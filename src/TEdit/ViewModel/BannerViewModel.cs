using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using TEdit.Render;
using TEdit.Reactive;
using TEdit.Terraria;

namespace TEdit.ViewModel;

public partial class BannerViewModel : ReactiveObject
{
    private readonly WorldViewModel _wvm;

    public BannerViewModel()
    {
        _wvm = ViewModelLocator.WorldViewModel;

        BannerData = new ObservableCollection<BannerItem>();
        Categories = new ObservableCollection<string> { Properties.Language.common_all };

        BannersView = CollectionViewSource.GetDefaultView(BannerData);
        BannersView.Filter = FilterBanners;
        BannersView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BannerItem.Category)));

        this.WhenAnyValue(x => x.FilterText, x => x.SelectedCategory)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOnMainThread()
            .Subscribe(_ => BannersView.Refresh());

        // Auto-populate banners when a world is opened
        _wvm.WhenAnyValue(x => x.CurrentWorld)
            .Where(w => w != null)
            .ObserveOnMainThread()
            .Subscribe(_ => PopulateBanners());

        NpcPreviewCache.PreviewsLoaded += OnNpcPreviewsLoaded;
    }

    private void OnNpcPreviewsLoaded()
    {
        foreach (var item in BannerData)
        {
            item.NotifyPreviewChanged();
        }
    }

    [Reactive]
    private ObservableCollection<BannerItem> _bannerData;

    [Reactive]
    private string _filterText = "";

    [Reactive]
    private string _selectedCategory = Properties.Language.common_all;

    public ObservableCollection<string> Categories { get; }

    public ICollectionView BannersView { get; }

    private bool FilterBanners(object obj)
    {
        if (obj is not BannerItem item) return false;

        if (!string.IsNullOrEmpty(SelectedCategory) &&
            SelectedCategory != Properties.Language.common_all &&
            !string.Equals(item.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(item.RawCategory, SelectedCategory, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(FilterText) &&
            !item.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
            !item.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
            !item.RawCategory.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
            !item.NpcId.ToString().Contains(FilterText, StringComparison.Ordinal) &&
            !item.BestiaryIndex.ToString().Contains(FilterText, StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    public void ApplySort(string propertyName)
    {
        var direction = ListSortDirection.Ascending;

        if (BannersView.SortDescriptions.Count > 0 &&
            BannersView.SortDescriptions[0].PropertyName == propertyName)
        {
            direction = BannersView.SortDescriptions[0].Direction == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }

        BannersView.SortDescriptions.Clear();
        BannersView.SortDescriptions.Add(new SortDescription(propertyName, direction));
    }

    /// <summary>
    /// Populates the banner list from the current world data.
    /// Called automatically when a world is loaded.
    /// </summary>
    private void PopulateBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        BannerData.Clear();
        Categories.Clear();
        Categories.Add(Properties.Language.common_all);

        // Build lookup: bannerId -> first NPC data
        var npcByBanner = WorldConfiguration.BestiaryData.NpcById.Values
            .Where(n => n.BannerId >= 0)
            .GroupBy(n => n.BannerId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var kvp in WorldConfiguration.TallyNames.OrderBy(k => k.Key))
        {
            int tallyIndex = kvp.Key;
            string bannerName = WorldConfiguration.TallyLocalizedNames.TryGetValue(tallyIndex, out var localizedBanner) ? localizedBanner : kvp.Value;

            // Get category, NPC ID, and bestiary index from bestiary data
            string category = "";
            int npcId = 0;
            int bestiaryIndex = 0;
            if (npcByBanner.TryGetValue(tallyIndex, out var npcData))
            {
                category = npcData.Category ?? "";
                npcId = npcData.Id;
                bestiaryIndex = npcData.BestiaryDisplayIndex;
            }

            // Read current values from world
            ushort bannerCount = 0;
            if (tallyIndex >= 0 && tallyIndex < _wvm.CurrentWorld.ClaimableBanners.Count)
            {
                bannerCount = _wvm.CurrentWorld.ClaimableBanners[tallyIndex];
            }

            int kills = 0;
            if (tallyIndex >= 0 && tallyIndex < _wvm.CurrentWorld.KilledMobs.Count)
            {
                kills = _wvm.CurrentWorld.KilledMobs[tallyIndex];
            }

            string rawCategory = string.IsNullOrEmpty(category) ? "Other" : category;
            BannerData.Add(new BannerItem
            {
                TallyIndex = tallyIndex,
                Name = bannerName,
                Category = GetCategoryDisplayName(rawCategory),
                RawCategory = rawCategory,
                NpcId = npcId,
                BestiaryIndex = bestiaryIndex,
                Kills = kills,
                Count = bannerCount,
            });
        }

        // Populate category filter list
        foreach (var cat in BannerData.Select(b => b.Category).Distinct().OrderBy(c => c))
        {
            if (!Categories.Contains(cat))
                Categories.Add(cat);
        }

        BannersView.Refresh();
    }

    /// <summary>
    /// Import banner data from a different world file.
    /// </summary>
    [ReactiveCommand]
    private async Task ImportBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            Properties.Language.dialog_import_banners_title,
            Properties.Language.dialog_import_banners_prompt);

        if (!confirm)
            return;

        var ofd = new OpenFileDialog
        {
            Filter = "Terraria World File|*.wld",
            DefaultExt = "Terraria World File|*.wld",
            Title = Properties.Language.dialog_import_banners,
            InitialDirectory = DependencyChecker.PathToWorlds,
            Multiselect = false,
        };

        if (ofd.ShowDialog() != true) return;

        // Backup current state
        var backup = _wvm.CurrentWorld.ClaimableBanners.ToArray();
        try
        {
            World.ImportBanners(_wvm.CurrentWorld, ofd.FileName);
            PopulateBanners();
        }
        catch (Exception ex)
        {
            // Restore backup
            for (int i = 0; i < backup.Length && i < _wvm.CurrentWorld.ClaimableBanners.Count; i++)
            {
                _wvm.CurrentWorld.ClaimableBanners[i] = backup[i];
            }
            await App.DialogService.ShowExceptionAsync(string.Format(Properties.Language.dialog_import_banners_error, ofd.FileName, ex.Message));
        }
    }

    [ReactiveCommand]
    private async Task SaveBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            Properties.Language.dialog_save_banners_title,
            Properties.Language.dialog_save_banners_prompt);

        if (!confirm)
            return;

        // Backup
        var backup = _wvm.CurrentWorld.ClaimableBanners.ToArray();
        try
        {
            foreach (var item in BannerData)
            {
                if (item.TallyIndex >= 0 && item.TallyIndex < _wvm.CurrentWorld.ClaimableBanners.Count)
                {
                    _wvm.CurrentWorld.ClaimableBanners[item.TallyIndex] = item.Count;
                }
            }
        }
        catch (Exception ex)
        {
            // Restore backup
            for (int i = 0; i < backup.Length && i < _wvm.CurrentWorld.ClaimableBanners.Count; i++)
            {
                _wvm.CurrentWorld.ClaimableBanners[i] = backup[i];
            }
            await App.DialogService.ShowExceptionAsync(string.Format(Properties.Language.dialog_save_banners_error, ex.Message));
        }
    }

    [ReactiveCommand]
    private async Task MaxOutBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            Properties.Language.dialog_max_banners_title,
            Properties.Language.dialog_max_banners_prompt);

        if (!confirm)
            return;

        foreach (var item in BannerData)
        {
            item.Count = 9999;
        }
    }

    [ReactiveCommand]
    private async Task ResetBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            Properties.Language.dialog_reset_banners_title,
            Properties.Language.dialog_reset_banners_prompt);

        if (!confirm)
            return;

        foreach (var item in BannerData)
        {
            item.Count = 0;
        }
    }

    private static string GetCategoryDisplayName(string category) => category switch
    {
        "Blood Moon" => Properties.Language.bestiary_cat_blood_moon,
        "Cavern" => Properties.Language.bestiary_cat_cavern,
        "Corruption" => Properties.Language.bestiary_cat_corruption,
        "Crimson" => Properties.Language.bestiary_cat_crimson,
        "Desert" => Properties.Language.bestiary_cat_desert,
        "Dungeon" => Properties.Language.bestiary_cat_dungeon,
        "Frost Legion" => Properties.Language.bestiary_cat_frost_legion,
        "Frost Moon" => Properties.Language.bestiary_cat_frost_moon,
        "Goblin Army" => Properties.Language.bestiary_cat_goblin_army,
        "Granite Cave" => Properties.Language.bestiary_cat_granite_cave,
        "Hallow" => Properties.Language.bestiary_cat_hallow,
        "Halloween" => Properties.Language.bestiary_cat_halloween,
        "Jungle" => Properties.Language.bestiary_cat_jungle,
        "Lunar Event" => Properties.Language.bestiary_cat_lunar_event,
        "Marble Cave" => Properties.Language.bestiary_cat_marble_cave,
        "Martian Madness" => Properties.Language.bestiary_cat_martian_madness,
        "Meteorite" => Properties.Language.bestiary_cat_meteorite,
        "Mushroom" => Properties.Language.bestiary_cat_mushroom,
        "Ocean" => Properties.Language.bestiary_cat_ocean,
        "Old One's Army" => Properties.Language.bestiary_cat_old_ones_army,
        "Pirate Invasion" => Properties.Language.bestiary_cat_pirate_invasion,
        "Pumpkin Moon" => Properties.Language.bestiary_cat_pumpkin_moon,
        "Rain" => Properties.Language.bestiary_cat_rain,
        "Sandstorm" => Properties.Language.bestiary_cat_sandstorm,
        "Slime" => Properties.Language.bestiary_cat_slime,
        "Snow" => Properties.Language.bestiary_cat_snow,
        "Solar Eclipse" => Properties.Language.bestiary_cat_solar_eclipse,
        "Spider Nest" => Properties.Language.bestiary_cat_spider_nest,
        "Surface" => Properties.Language.bestiary_cat_surface,
        "Temple" => Properties.Language.bestiary_cat_temple,
        "Underworld" => Properties.Language.bestiary_cat_underworld,
        "Windy Day" => Properties.Language.bestiary_cat_windy_day,
        "Other" => Properties.Language.common_other,
        _ => category
    };
}

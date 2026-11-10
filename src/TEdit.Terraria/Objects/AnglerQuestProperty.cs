namespace TEdit.Terraria.Objects;

public class AnglerQuestProperty
{
    public int QuestId { get; set; }
    public int ItemId { get; set; }
    public ItemProperty? Item { get; set; }
    public string Name => Item?.Name ?? ItemId.ToString();

    public override string ToString() => Name;
}

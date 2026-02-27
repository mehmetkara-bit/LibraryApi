namespace LibraryApi.Contracts.Categories;

public class CategoryListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
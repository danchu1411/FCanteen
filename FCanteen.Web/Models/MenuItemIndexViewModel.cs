using FCanteen.Data.Entities;

namespace FCanteen.Web.Models;

public class MenuItemIndexViewModel
{
    public IReadOnlyList<MenuItem>
        MenuItems
    {
        get;
        set;
    } = [];

    public IReadOnlyList<Category>
        Categories
    {
        get;
        set;
    } = [];

    /*
     * Current search/filter/sort state.
     */
    public string? SearchTerm
    {
        get;
        set;
    }

    public int? CategoryId
    {
        get;
        set;
    }

    public bool? IsAvailable
    {
        get;
        set;
    }

    public string SortBy
    {
        get;
        set;
    } = "name_asc";

    /*
     * Paging.
     */
    public int Page
    {
        get;
        set;
    }

    public int PageSize
    {
        get;
        set;
    }

    public int TotalCount
    {
        get;
        set;
    }

    public int TotalPages
    {
        get;
        set;
    }

    public bool HasPreviousPage =>
        Page > 1;

    public bool HasNextPage =>
        Page < TotalPages;

    public int FirstItemNumber =>
        TotalCount == 0
            ? 0
            : (Page - 1) *
              PageSize + 1;

    public int LastItemNumber =>
        Math.Min(
            Page * PageSize,
            TotalCount);
}

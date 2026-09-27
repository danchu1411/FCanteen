using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace FCanteen.Repositories.Implementations;

public class CategoryRepository
    : ICategoryRepository
{
    private readonly FCanteenContext _db;

    public CategoryRepository(
        FCanteenContext db)
    {
        _db = db;
    }

    public async Task<
        IReadOnlyList<Category>>
        GetAllAsync(
            CancellationToken cancellationToken =
                default)
    {
        return await _db.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(
                cancellationToken);
    }
}

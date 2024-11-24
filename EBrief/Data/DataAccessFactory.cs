using EBrief.Shared.Data;
using Microsoft.Extensions.DependencyInjection;

namespace EBrief.Data;
public class DataAccessFactory : IDataAccessFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DataAccessFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IDataAccess Create()
    {
        return _serviceProvider.GetRequiredService<IDataAccess>();
    }
}

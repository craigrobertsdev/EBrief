using EBrief.Shared.Data;
using Microsoft.Extensions.DependencyInjection;

namespace EBrief.Data;
public class FileServiceFactory : IFileServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FileServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IFileService Create()
    {
        return _serviceProvider.GetRequiredService<IFileService>();
    }
}

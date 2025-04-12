using System.Threading;
using System.Threading.Tasks;

namespace HLE.Threading;

public delegate Task AsyncEventHandler<in TSender>(TSender sender, CancellationToken cancellationToken = default);

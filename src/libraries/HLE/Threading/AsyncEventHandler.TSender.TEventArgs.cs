using System.Threading.Tasks;

namespace HLE.Threading;

public delegate Task AsyncEventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs args);

using System.Linq;
using System.Reflection;
using Ninject;

namespace PeMirror.Injections.Extensions
{
    public static class KernelExtensions
    {
        public static IKernel InjectPixelExperienceKernel(this IKernel kernel)
        {
            var types = Assembly.GetAssembly(typeof(KernelExtensions)).GetTypes()
                .Where(i => i.Namespace != null && i.Namespace == "PeMirror.Injections").ToList();

            foreach (var type in types) kernel.Bind(type).ToSelf();

            return kernel;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloner.Universal.Registries
{
    /// <summary>
    /// Defines a registry that takes care of registering
    /// multiple dependencies to a dependency container.
    /// </summary>
    public interface IRegistry
    {
        /// <summary>
        /// Configures dependencies.
        /// </summary>
        void Configure();
    }
}

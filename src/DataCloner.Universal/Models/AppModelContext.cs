using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloner.Universal.Models
{
    public class AppModelContext : IAppContext
    {
        public string CurrentFilePath { get; set; }

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        public static AppModelContext Instance
        {
            get { return ServiceLocator.Current.GetInstance<IAppContext>() as AppModelContext; }
        }
    }
}

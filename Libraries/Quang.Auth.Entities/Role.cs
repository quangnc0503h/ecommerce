using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public class Role
    {
      
        /// <summary>
        /// Role ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        public string Name { get; set; }

        public TRole ToRoleModel<TRole>() where TRole : Role
        {
            object[] parameters = new object[] { Name, Id };
            var roleModel = Activator.CreateInstance(typeof(TRole), parameters) as TRole;
            return roleModel;
        }
    }
}

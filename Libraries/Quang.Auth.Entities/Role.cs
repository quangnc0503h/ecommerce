using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public class Role : IRole<int>
    {
        public Role()
        {
            //Id = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// Constructor that takes names as argument 
        /// </summary>
        /// <param name="name"></param>
        public Role(string name) : this()
        {
            Name = name;
        }

        public Role(string name, int id)
        {
            Name = name;
            Id = id;
        }
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

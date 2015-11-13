using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Cate.Entities
{
    public class Khuvuc
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public bool Published { get; set; }

        public int DisplayOrder { get; set; }

        public string Description { get; set; }

        public bool Deleted { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public string UserName { get; set; }
    }
}

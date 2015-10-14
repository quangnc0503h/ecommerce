namespace Quang.Cate.Entities
{
    public class Country
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public bool Published { get; set; }

        public int DisplayOrder { get; set; }

        public string Description { get; set; }
    }
}

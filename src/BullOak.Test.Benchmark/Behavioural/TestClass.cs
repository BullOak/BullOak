namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IInterface
    {
        int Count { get; set; }
        string Name { get; set; }
    }

    internal class TestClass : IInterface
    {
        public bool canEdit = false;

        private int _count;
        public int Count
        {
            get { return _count; }
            set
            {
                if (canEdit) _count = value;
                else
                    throw new Exception("You can only edit this item during reconstitution");
            }
        }

        public string Name { get; set; }
    }
}

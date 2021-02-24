using System.Collections.Generic;

namespace Neo4jOgm.Domain
{
    public class Sort
    {
        public List<Order> Orders { get; } = new();

        public bool HasOrders()
        {
            return Orders.Count > 0;
        }
    }

    public class Order
    {
        public string Property { get; set; }

        public Direction Direction { get; set; } = Direction.Desc;
    }

    public enum Direction
    {
        Asc,

        Desc
    }
}
using System.Collections.Generic;
using System.ComponentModel;

namespace Neo4jOgm.Domain
{
    public interface ICriteria
    {
    }

    public class Criteria : ICriteria
    {
        public const string ID = "ID";

        public List<ICriteria> Chain { get; } = new();

        public string QueryKey { get; internal set; }

        public string Property { get; }

        public Operator Operator { get; }

        public object Value { get; }

        public bool IsEmpty { get; }

        public Criteria()
        {
            IsEmpty = true;
        }
        
        public Criteria(string property, Operator @operator, object value)
        {
            Property = property;
            Operator = @operator;
            Value = value;
            IsEmpty = false;
            Chain.Add(this);
        }

        public Criteria(string queryKey, string property, Operator @operator, object value)
        {
            QueryKey = queryKey;
            Property = property;
            Operator = @operator;
            Value = value;
            IsEmpty = false;
            Chain.Add(this);
        }

        public Criteria Add(Criteria other)
        {
            Chain.Add(other);
            return this;
        }

        public Criteria And(Criteria other)
        {
            return AddToChain(BooleanOperator.__And, other);
        }

        public Criteria Or(Criteria other)
        {
            return AddToChain(BooleanOperator.__Or, other);
        }

        public Criteria OrNot(Criteria other)
        {
            Chain.Add(BooleanOperator.__Or);
            return AddToChain(BooleanOperator.__Not, other);
        }

        public Criteria Xor(Criteria other)
        {
            return AddToChain(BooleanOperator.__Xor, other);
        }

        public Criteria Not(Criteria other)
        {
            return AddToChain(BooleanOperator.__Not, other);
        }

        private Criteria AddToChain(BooleanOperator @operator, Criteria other)
        {
            Chain.Add(@operator);
            Chain.Add(other);
            return this;
        }
    }

    public class BooleanOperator : ICriteria
    {
        internal static readonly BooleanOperator __And = new("AND");
        internal static readonly BooleanOperator __Or = new("OR");
        internal static readonly BooleanOperator __Xor = new("XOR");
        internal static readonly BooleanOperator __Not = new("NOT");

        public string Operator { get; }

        private BooleanOperator(string @operator)
        {
            Operator = @operator;
        }
    }

    public enum Operator
    {
        [Description("=")] Equal,

        [Description("<")] Lt,

        [Description("<=")] Lte,

        [Description(">")] Gt,

        [Description(">=")] Gte
    }
}
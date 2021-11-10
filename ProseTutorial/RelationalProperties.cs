using Microsoft.ProgramSynthesis.VersionSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelationalProperties
{
    public interface IRelationalProperty
    {
        IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output);
    }

    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class RelationalPropertyAttribute : Attribute
    {
        private readonly string _name;
        private readonly Type _type;
        public string Name => _name;
        public Type Type => _type;
        public RelationalPropertyAttribute(string name, Type type)
        {
            _name = name;
            _type = type;
        }

    }


    public class RelationalApplicationStrategy : ApplicationStrategy
    {
        private Dictionary<RelationalPropertyAttribute, IRelationalProperty> _properties;
        public int TimeoutMillilseconds { get; set; } = 60 * 1000; // 1 minute is default max time
        public RelationalApplicationStrategy(string grammar) : base(grammar)
        {
            _properties = new Dictionary<RelationalPropertyAttribute, IRelationalProperty>();
        }

        private void CollectProperties()
        {
            var typesWithAttr =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(RelationalPropertyAttribute), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<RelationalPropertyAttribute>() };

            foreach(var t in typesWithAttr)
            {
                var prop = (IRelationalProperty) Activator.CreateInstance(t.Type);
                _properties.Add(t.Attributes.First(), prop);
            }
        }
        public ProgramSet GetProgramSet(IEnumerable<Tuple<object, object>> examples)
        {
            if (_properties.Count == 0)
                CollectProperties();

            return GetProgramSet(examples, new HashSet<IRelationalProperty>(_properties.Values));
        }

        public ProgramSet GetProgramSet(IEnumerable<Tuple<object, object>> examples, HashSet<IRelationalProperty> properties)
        {
            // 1. Collect all individual, satisfiable properties into a set, R_app
            var rApp = new HashSet<IRelationalProperty>();
            foreach(var prop in properties)
            {
                var set = GetProgramSetTimed(examples, new HashSet<IRelationalProperty>() { prop }, TimeoutMillilseconds);
                if(!set.IsEmpty)
                {
                    rApp.Add(prop);
                }
            }

            // 1.a. If only one or zero, apply simple property selection
            if (rApp.Count <= 1)
            {
                return base.GetProgramSet(examples, rApp);
            }


            // 2. Create a set of singleton sets, R_sat, one for each of the properties found in 1.
            var rSat = rApp.Select(x => new HashSet<IRelationalProperty>() { x }).ToHashSet();

            // 2.a. Create an empty conflict mapping
            var conflict = new HashSet<Tuple<IRelationalProperty, IRelationalProperty>>();

            // 3. While there's more than one set in R_sat
            while (rSat.Count > 1)
            {
                var newRSat = new HashSet<HashSet<IRelationalProperty>>();
                // 4. Go through each set in R_sat, named R_oldsat
                foreach (var oldSat in rSat)
                {
                    // 5. Create a new set R_new for each R from R_app that is not in R_oldsat
                    var rNewSet = rApp.Where(x => !oldSat.Contains(x)).ToHashSet();

                    foreach(var rNew in rNewSet)
                    {
                        // 6. Skip if there's a conflict in the conflict mapping between R_new and any R in R_oldsat
                        var pairings = oldSat.Select(x => Tuple.Create(x, rNew));
                        if (pairings.Any(conflict.Contains))
                        {
                            continue;
                        }

                        // 7. Attempt to get the program set for the new set 
                        var unionSet = new HashSet<IRelationalProperty>(oldSat);
                        unionSet.Add(rNew);
                        var set = GetProgramSetTimed(examples, unionSet, TimeoutMillilseconds);
                        if (!set.IsEmpty)
                        {
                            newRSat.Add(unionSet);
                        }
                        else
                        {
                            // 8. Failed to get program set, add these two properties to the conflict set
                            if (oldSat.Count == 1)
                            {
                                foreach(var r in oldSat)
                                {
                                    conflict.Add(Tuple.Create(r, rNew));
                                    conflict.Add(Tuple.Create(rNew, r));
                                }
                            }
                        }
                    }
                }
                if (newRSat.Count == 0)
                    break;

                rSat = newRSat;
            }


            return base.GetProgramSet(examples, Rank(rSat));
        }

        private HashSet<IRelationalProperty> Rank(HashSet<HashSet<IRelationalProperty>> properties)
        {
            var r = new Random();
            var randIdx = r.Next(0, properties.Count);
            return properties.ElementAt(randIdx);
        }
    }
}

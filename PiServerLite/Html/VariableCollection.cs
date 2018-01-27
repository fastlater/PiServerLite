﻿using PiServerLite.Extensions;
using System;
using System.Collections.Generic;

namespace PiServerLite.Html
{
    internal class VariableCollection
    {
        private readonly IDictionary<string, object> collection;

        public object this[string key] {
            get => collection[key];
            set => collection[key] = value;
        }


        public VariableCollection()
        {
            collection = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public VariableCollection(VariableCollection sourceCollection)
        {
            collection = new Dictionary<string, object>(sourceCollection.collection, StringComparer.OrdinalIgnoreCase);
        }

        public VariableCollection(object parameters)
        {
            collection = ObjectExtensions.ToDictionary(parameters);
        }

        /// <summary>
        /// Gets the value of a contained object or property
        /// using a fully-qualified path.
        /// </summary>
        public virtual bool TryGetValue(string key, out object value)
        {
            var keySegments = key.Split('.');
            var rootSegment = keySegments[0];

            if (!collection.TryGetValue(rootSegment, out value))
                return false;

            for (var i = 1; i < keySegments.Length; i++) {
                if (value == null) return false;

                var segment = keySegments[i];

                var xType = value.GetType();

                var xField = xType.GetField(segment);
                if (xField != null) {
                    value = xField.GetValue(value);
                    continue;
                }

                var xProperty = xType.GetProperty(segment);
                if (xProperty != null) {
                    value = xProperty.GetValue(value);
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}

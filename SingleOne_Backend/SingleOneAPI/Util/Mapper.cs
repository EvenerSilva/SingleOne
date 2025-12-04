using System.Collections.Generic;
using System.Linq;

namespace SingleOneAPI.Util
{
    public static class Mapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            var destination = new TDestination();
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            foreach (var sourceProperty in sourceType.GetProperties())
            {
                var destinationProperty = destinationType.GetProperty(sourceProperty.Name);
                if (destinationProperty != null && destinationProperty.CanWrite && destinationProperty.PropertyType == sourceProperty.PropertyType)
                {
                    destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
                }
            }

            return destination;
        }

        public static List<TDestination> Map<TSource, TDestination>(List<TSource> sourceList)
            where TDestination : new()
        {
            return sourceList.Select(source => Map<TSource, TDestination>(source)).ToList();
        }
    }
}

using System.Collections.Generic;

namespace Blackjack
{
    public class StrategyManager
    {
        private readonly Dictionary<double, Strategy> _strategies = new();
        private readonly Strategy _defaultStrategy;

        public StrategyManager()
        {
            _defaultStrategy = Strategy.Instance;
        }

        public void AddOrUpdateStrategy(double localUnit, Strategy strategy)
        {
            _strategies[localUnit] = strategy;
        }

        public Strategy GetStrategy(double localUnit)
        {
            return _strategies.GetValueOrDefault(localUnit, _defaultStrategy);
        }

        public IEnumerable<double> GetLocalUnits()
        {
            return _strategies.Keys.OrderBy(k => k);
        }
        public IReadOnlyDictionary<double, Strategy> GetAllStrategies()
        {
            return _strategies;
        }
    }
}
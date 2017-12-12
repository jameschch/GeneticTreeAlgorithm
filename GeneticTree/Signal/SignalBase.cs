using QuantConnect.Data;
using QuantConnect.Indicators;

namespace GeneticTree.Signal
{
    public abstract class SignalBase : ISignal
    {

        public ISignal Child { get; set; }

        public ISignal Parent { get; set; }

        public Operator Operator { get; set; }

        public abstract bool IsReady { get; }

        public string Name { get; set; }

        public abstract bool IsTrue();

        public abstract decimal Value { get; }

        public virtual void Update(BaseData data)
        {
            if (Child != null)
            {
                Child.Update(data);
            }
        }

        protected RollingWindow<int> SurvivalWindow;

    }
}

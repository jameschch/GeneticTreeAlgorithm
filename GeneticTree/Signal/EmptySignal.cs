using QuantConnect.Data;

namespace GeneticTree.Signal
{

    public class EmptySignal : ISignal
    {

        public bool _isTrue = true;

        public EmptySignal(bool isTrue = true)
        {
            _isTrue = isTrue;
        }

        public bool IsReady { get { return true; } }

        public Operator Operator { get; set; }

        public ISignal Child { get; set; }

        public ISignal Parent { get; set; }

        public string Name { get; set; }

        public bool IsTrue()
        {
            return _isTrue;
        }

        public void Update(BaseData data)
        {
        }
    }
}

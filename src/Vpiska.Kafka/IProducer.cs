using System.Threading.Tasks;

namespace Vpiska.Kafka
{
    public interface IProducer<in TEvent>
    {
        ValueTask ProduceAsync(TEvent queueEvent);
    }
}
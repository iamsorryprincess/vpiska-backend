namespace Vpiska.Kafka.Settings
{
    public sealed class KafkaConsumerSettings<TEvent> : KafkaSettings
    {
        public string GroupId { get; set; }
    }
}
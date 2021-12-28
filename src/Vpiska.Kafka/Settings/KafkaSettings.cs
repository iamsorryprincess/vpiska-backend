namespace Vpiska.Kafka.Settings
{
    public abstract class KafkaSettings
    {
        public string Servers { get; set; }

        public string Topic { get; set; }
    }
}
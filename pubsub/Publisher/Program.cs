using EasyNetQ;
using Messages;
string AMQP = "amqps://vkkvkgxw:TJDP2yuJisg98yXtSe2tD2A4LzPFX6RU@hippy-red-gecko.rmq3.cloudamqp.com/vkkvkgxw";
var bus = RabbitHutch.CreateBus(AMQP);

while(true)
{
    Console.WriteLine("Press any key to send a message");
    Console.ReadKey();
    var msg = @"
▄████████▄
██████████
█▄██████▄█
██▄▄▄▄▄▄██
  ██████
";
    var message = new Message(msg);
    bus.PubSub.Publish(message);
}

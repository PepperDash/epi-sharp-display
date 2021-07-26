using System;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Sharp.DisplayEventArgs;


namespace PepperDash.Essentials.Sharp.SharpPluginQueue
{
    public class SharpDisplayPluginQueue
    {
        readonly CrestronQueue<SharpDisplayCommand> _pluginQueue;


        public EventHandler<SharpDisplayMessageEventArgs> MessageProcessed;

        public SharpDisplayPluginQueue()
        {
            _pluginQueue = new CrestronQueue<SharpDisplayCommand>(25);
            //PluginThread = new Thread(o => ProcessQueue(), null, Thread.eThreadStartOptions.Running);
        }

       

        public void RaiseEvent_ProcessMessage(string message)
        {
            var handler = MessageProcessed;
            if (handler != null)
            {
                handler(this, new SharpDisplayMessageEventArgs(message));
            }
        }

        public SharpDisplayCommand GetNextCommand()
        {
            return _pluginQueue.IsEmpty ? null : _pluginQueue.Peek();
        }


        public void EnqueueMessage(SharpDisplayCommand response)
        {
            _pluginQueue.Enqueue(response);
        }

        

        public SharpDisplayCommand DequeueMessage()
        {
            return _pluginQueue.TryToDequeue();
            
        }

        public void PrintQueue()
        {
            foreach (var cmd in _pluginQueue)
            {
                Debug.Console(2, "In Queue: {0}", cmd.Command);
            }
        }

        public void ClearQueue()
        {
            _pluginQueue.Clear();
        }
    }
}
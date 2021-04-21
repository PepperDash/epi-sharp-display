using System;
using Crestron.SimplSharp;
using Epi.Display.Sharp.DisplayEventArgs;
using PepperDash.Core;


namespace Epi.Display.Sharp.SharpPluginQueue
{
    public class SharpDisplayPluginQueue
    {
        readonly CrestronQueue<SharpDisplayCommand> PluginQueue;


        public EventHandler<SharpDisplayMessageEventArgs> MessageProcessed;

        public SharpDisplayPluginQueue()
        {
            PluginQueue = new CrestronQueue<SharpDisplayCommand>(25);
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
            return PluginQueue.IsEmpty ? null : PluginQueue.Peek();
        }


        public void EnqueueMessage(SharpDisplayCommand response)
        {
            PluginQueue.Enqueue(response);
        }

        

        public SharpDisplayCommand DequeueMessage()
        {
            return PluginQueue.TryToDequeue();
            
        }

        public void PrintQueue()
        {
            foreach (var cmd in PluginQueue)
            {
                Debug.Console(2, "In Queue: {0}", cmd.Command);
            }
        }

        public void ClearQueue()
        {
            PluginQueue.Clear();
        }
    }
}
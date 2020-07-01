using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharp;

using PepperDash.Core;

using Epi.Display.Sharp.DisplayEventArgs;


namespace Epi.Display.Sharp.SharpPluginQueue
{
    public class SharpDisplayPluginQueue
    {
        CrestronQueue<SharpDisplayPluginMessage> PluginQueue;
        //Thread PluginThread;

        public EventHandler<SharpDisplayMessageEventArgs> MessageProcessed;

        public SharpDisplayPluginQueue()
        {
            PluginQueue = new CrestronQueue<SharpDisplayPluginMessage>(25);
            //PluginThread = new Thread(o => ProcessQueue(), null, Thread.eThreadStartOptions.Running);
        }

       

        public void RaiseEvent_ProcessMessage(string message)
        {
            var Handler = MessageProcessed;
            if (Handler != null)
            {
                Handler(this, new SharpDisplayMessageEventArgs(message));
            }
        }

        public SharpDisplayPluginMessage GetNextCommand()
        {
            if (PluginQueue.IsEmpty)
                return null;

            return PluginQueue.Peek();
        }


        public void EnqueueMessage(SharpDisplayPluginMessage response)
        {
            PluginQueue.Enqueue(response);
        }

        public SharpDisplayPluginMessage DequeueMessage()
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
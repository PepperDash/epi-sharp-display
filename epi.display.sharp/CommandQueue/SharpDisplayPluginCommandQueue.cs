using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Epi.SharpCommandTimer;
using Epi.Display.Sharp.DisplayEventArgs;

namespace Epi.Display.Sharp.CommandQueue
{
    public class SharpDisplayPluginCommandQueue
    {
        CrestronQueue CommandQueue = new CrestronQueue();
        CommandTimer CommandTimer = new CommandTimer();

        SharpDisplayPluginDevice Device;
        ushort CommandQueueTimeOutValue;

        public bool CommandQueueInProgress;


        public SharpDisplayPluginCommandQueue(SharpDisplayPluginDevice device, ushort timeOutValue)
        {
            Device = device;

            CommandQueueTimeOutValue = timeOutValue;
        }


        public void EnqueueCommand(string command)
        {
            CommandQueue.Enqueue(command);
            if (!CommandQueueInProgress)
                SendNextQueuedCommand();
        }

        public void SendNextQueuedCommand()
        {
            if (!CommandQueue.IsEmpty)
            {
                CommandTimer.StartTimer(CommandQueueTimeOutValue);

                CommandQueueInProgress = true;

                if (CommandQueue.Peek() is string)
                {
                    string NextCommand = (string)CommandQueue.Dequeue();
                    Device.SendLine(NextCommand);
                }
            }
            else
            {
                CommandQueueInProgress = false;
                CommandTimer.StopTimer();
            }
        }

        public void OnValidResponseReceived(object sender, EventArgs e)
        {
            CommandTimer.StartTimer(CommandQueueTimeOutValue);
            SendNextQueuedCommand();
        }

        public void OnTimerCompleted(object sender, EventArgs e)
        {
            CommandQueueInProgress = false;
            SendNextQueuedCommand();
        }
    }
}
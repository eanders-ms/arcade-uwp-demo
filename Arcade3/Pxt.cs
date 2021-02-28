using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;
using Windows.Storage;

namespace Arcade3
{
    class ControllerState
    {
        public enum PxtKeyCode
        {
            KEY_LEFT = 1,
            KEY_UP,
            KEY_RIGHT,
            KEY_DOWN,
            KEY_A,
            KEY_B,
            KEY_MENU,
            KEY_RESET = 100, // passed as event to TS, which does control.reset()
            KEY_EXIT,        // handled here
        };


        public enum PxtKeyState
        {
            IDLE = 0,
            INTERNAL_KEY_UP = 2050,
            INTERNAL_KEY_DOWN = 2051
        }

        int playerId;
        readonly Dictionary<PxtKeyCode, PxtKeyState> keys = new Dictionary<PxtKeyCode, PxtKeyState>();

        public ControllerState(int playerId)
        {
            this.playerId = playerId;
            keys[PxtKeyCode.KEY_LEFT] = PxtKeyState.IDLE;
            keys[PxtKeyCode.KEY_RIGHT] = PxtKeyState.IDLE;
            keys[PxtKeyCode.KEY_UP] = PxtKeyState.IDLE;
            keys[PxtKeyCode.KEY_DOWN] = PxtKeyState.IDLE;
            keys[PxtKeyCode.KEY_A] = PxtKeyState.IDLE;
            keys[PxtKeyCode.KEY_B] = PxtKeyState.IDLE;
        }
        public void UpdateKey(PxtKeyCode code, bool state)
        {
            if (state == false) // up
            {
                if (keys[code] == PxtKeyState.INTERNAL_KEY_DOWN)
                {
                    keys[code] = PxtKeyState.INTERNAL_KEY_UP;
                }
                else
                {
                    keys[code] = PxtKeyState.IDLE;
                }
            }
            else
            {
                keys[code] = PxtKeyState.INTERNAL_KEY_DOWN;
            }
        }

        public void Send()
        {
            foreach (var kv in keys)
            {
                if (kv.Value != PxtKeyState.IDLE)
                {
                    Pxt.VmRaiseEvent((int)kv.Value, (int)kv.Key + 7 * playerId);
                    Pxt.VmRaiseEvent((int)kv.Value, 0); // any
                }
            }
        }
    }

    public class Pxt
    {
        private const string DllPath = "pxt";

        public const int WIDTH = 160;
        public const int HEIGHT = 120;
        public const int BUF_SIZE_IN_BYTES = WIDTH * HEIGHT * 4;
        public const int BUF_SIZE_IN_PIXELS = WIDTH * HEIGHT;

        const int LOG_SZ = 4096;
        byte[] logs = new byte[LOG_SZ];

        private IntPtr buffer;

        private readonly ControllerState player1 = new ControllerState(0);
        private readonly ControllerState player2 = new ControllerState(1);

        public Pxt()
        {
        }

        public void Initialize()
        {
            VmSetDataDirectory(ApplicationData.Current.LocalFolder.Path);
            ReadLogs();
        }

        public void LoadContent()
        {
            buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * BUF_SIZE_IN_PIXELS);
            var path = Game1.Instance.Content.RootDirectory + "/roms/castle-crawler.pxt64\0";
            var bytes = System.Text.Encoding.ASCII.GetBytes(path);
            VmStart(bytes);
            ReadLogs();
        }

        public void UnloadContent()
        {
            Marshal.FreeCoTaskMem(buffer);
            buffer = IntPtr.Zero;
        }

        public void Update()
        {
            UpdateControllerStates();
            SendInput();
            ReadLogs();
        }

        public void SendInput()
        {
            player1.Send();
            player2.Send();
        }

        public void GetPixels(int[] pixels)
        {
            VmGetPixels(WIDTH, HEIGHT, buffer);
            Marshal.Copy(buffer, pixels, 0, BUF_SIZE_IN_PIXELS);
        }

        private void UpdateControllerStates()
        {
            var kbd = Keyboard.GetState();

            player1.UpdateKey(ControllerState.PxtKeyCode.KEY_UP, kbd.IsKeyDown(Keys.W) || kbd.IsKeyDown(Keys.Up));
            player1.UpdateKey(ControllerState.PxtKeyCode.KEY_LEFT, kbd.IsKeyDown(Keys.A) || kbd.IsKeyDown(Keys.Left));
            player1.UpdateKey(ControllerState.PxtKeyCode.KEY_DOWN, kbd.IsKeyDown(Keys.S) || kbd.IsKeyDown(Keys.Down));
            player1.UpdateKey(ControllerState.PxtKeyCode.KEY_RIGHT, kbd.IsKeyDown(Keys.D) || kbd.IsKeyDown(Keys.Right));
            player1.UpdateKey(ControllerState.PxtKeyCode.KEY_A, kbd.IsKeyDown(Keys.Space) || kbd.IsKeyDown(Keys.Q) || kbd.IsKeyDown(Keys.Z));
            player1.UpdateKey(ControllerState.PxtKeyCode.KEY_B, kbd.IsKeyDown(Keys.Enter) || kbd.IsKeyDown(Keys.X) || kbd.IsKeyDown(Keys.E));

            player2.UpdateKey(ControllerState.PxtKeyCode.KEY_UP, kbd.IsKeyDown(Keys.I));
            player2.UpdateKey(ControllerState.PxtKeyCode.KEY_LEFT, kbd.IsKeyDown(Keys.J));
            player2.UpdateKey(ControllerState.PxtKeyCode.KEY_DOWN, kbd.IsKeyDown(Keys.L));
            player2.UpdateKey(ControllerState.PxtKeyCode.KEY_RIGHT, kbd.IsKeyDown(Keys.K));
            player2.UpdateKey(ControllerState.PxtKeyCode.KEY_A, kbd.IsKeyDown(Keys.U));
            player2.UpdateKey(ControllerState.PxtKeyCode.KEY_B, kbd.IsKeyDown(Keys.O));
        }

        private void ReadLogs()
        {
            VmGetLogs(0, logs, LOG_SZ);
            string result = System.Text.Encoding.UTF8.GetString(logs);
            System.Diagnostics.Debug.WriteLine(result);
        }

        [DllImport(DllPath, EntryPoint = "pxt_screen_get_pixels", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern void VmGetPixels(int width, int height, IntPtr screen);

        [DllImport(DllPath, EntryPoint = "pxt_raise_event", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern void VmRaiseEvent(int src, int val);

        [DllImport(DllPath, EntryPoint = "pxt_vm_start", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern void VmStart([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] byte[] path);

        [DllImport(DllPath, EntryPoint = "pxt_vm_start_buffer", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern void VmStartBuffer(byte[] buffer, int size);

        [DllImport(DllPath, EntryPoint = "pxt_get_logs", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern int VmGetLogs(int logtype, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] byte[] dst, int maxSize);

        [DllImport(DllPath, EntryPoint = "pxt_vm_set_data_directory", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern void VmSetDataDirectory(string path);
    }
}
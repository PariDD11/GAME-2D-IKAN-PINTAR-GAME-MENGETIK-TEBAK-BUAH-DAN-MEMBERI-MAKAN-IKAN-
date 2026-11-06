using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace IkanPintar
{
    public enum GameKey
    {
        Left, Right, Up, Down,
        W, A, S, D, B, C,
        Enter, Space, Escape, Backspace,
        Alpha1, Alpha2, Alpha3, Alpha4
    }

    public static class GameInput
    {
        static readonly List<char> typed = new List<char>();

#if ENABLE_INPUT_SYSTEM
        static Keyboard hooked;

        static void OnChar(char c)
        {
            if (typed.Count < 64) typed.Add(c);
        }

        static ButtonControl Primary(Keyboard kb, GameKey k)
        {
            switch (k)
            {
                case GameKey.Left: return kb.leftArrowKey;
                case GameKey.Right: return kb.rightArrowKey;
                case GameKey.Up: return kb.upArrowKey;
                case GameKey.Down: return kb.downArrowKey;
                case GameKey.W: return kb.wKey;
                case GameKey.A: return kb.aKey;
                case GameKey.S: return kb.sKey;
                case GameKey.D: return kb.dKey;
                case GameKey.B: return kb.bKey;
                case GameKey.C: return kb.cKey;
                case GameKey.Enter: return kb.enterKey;
                case GameKey.Space: return kb.spaceKey;
                case GameKey.Escape: return kb.escapeKey;
                case GameKey.Backspace: return kb.backspaceKey;
                case GameKey.Alpha1: return kb.digit1Key;
                case GameKey.Alpha2: return kb.digit2Key;
                case GameKey.Alpha3: return kb.digit3Key;
                case GameKey.Alpha4: return kb.digit4Key;
                default: return null;
            }
        }

        static ButtonControl Secondary(Keyboard kb, GameKey k)
        {
            switch (k)
            {
                case GameKey.Enter: return kb.numpadEnterKey;
                case GameKey.Alpha1: return kb.numpad1Key;
                case GameKey.Alpha2: return kb.numpad2Key;
                case GameKey.Alpha3: return kb.numpad3Key;
                case GameKey.Alpha4: return kb.numpad4Key;
                default: return null;
            }
        }

        public static bool WasPressed(GameKey k)
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return false;
            ButtonControl a = Primary(kb, k);
            if (a != null && a.wasPressedThisFrame) return true;
            ButtonControl b = Secondary(kb, k);
            return b != null && b.wasPressedThisFrame;
        }

        public static bool IsHeld(GameKey k)
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return false;
            ButtonControl a = Primary(kb, k);
            if (a != null && a.isPressed) return true;
            ButtonControl b = Secondary(kb, k);
            return b != null && b.isPressed;
        }

        public static Vector2 PointerPosition
        {
            get
            {
                Touchscreen ts = Touchscreen.current;
                if (ts != null && ts.primaryTouch.press.isPressed) return ts.primaryTouch.position.ReadValue();
                Mouse m = Mouse.current;
                if (m != null) return m.position.ReadValue();
                if (ts != null) return ts.primaryTouch.position.ReadValue();
                return Vector2.zero;
            }
        }

        public static bool PointerPressed
        {
            get
            {
                Mouse m = Mouse.current;
                if (m != null && m.leftButton.wasPressedThisFrame) return true;
                Touchscreen ts = Touchscreen.current;
                return ts != null && ts.primaryTouch.press.wasPressedThisFrame;
            }
        }

        public static void Poll()
        {
            Keyboard kb = Keyboard.current;
            if (kb != hooked)
            {
                if (hooked != null) hooked.onTextInput -= OnChar;
                hooked = kb;
                if (hooked != null) hooked.onTextInput += OnChar;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            hooked = null;
            typed.Clear();
        }
#else
        static KeyCode Map(GameKey k)
        {
            switch (k)
            {
                case GameKey.Left: return KeyCode.LeftArrow;
                case GameKey.Right: return KeyCode.RightArrow;
                case GameKey.Up: return KeyCode.UpArrow;
                case GameKey.Down: return KeyCode.DownArrow;
                case GameKey.W: return KeyCode.W;
                case GameKey.A: return KeyCode.A;
                case GameKey.S: return KeyCode.S;
                case GameKey.D: return KeyCode.D;
                case GameKey.B: return KeyCode.B;
                case GameKey.C: return KeyCode.C;
                case GameKey.Enter: return KeyCode.Return;
                case GameKey.Space: return KeyCode.Space;
                case GameKey.Escape: return KeyCode.Escape;
                case GameKey.Backspace: return KeyCode.Backspace;
                case GameKey.Alpha1: return KeyCode.Alpha1;
                case GameKey.Alpha2: return KeyCode.Alpha2;
                case GameKey.Alpha3: return KeyCode.Alpha3;
                default: return KeyCode.Alpha4;
            }
        }

        static KeyCode MapAlt(GameKey k)
        {
            switch (k)
            {
                case GameKey.Enter: return KeyCode.KeypadEnter;
                case GameKey.Alpha1: return KeyCode.Keypad1;
                case GameKey.Alpha2: return KeyCode.Keypad2;
                case GameKey.Alpha3: return KeyCode.Keypad3;
                case GameKey.Alpha4: return KeyCode.Keypad4;
                default: return KeyCode.None;
            }
        }

        public static bool WasPressed(GameKey k)
        {
            if (Input.GetKeyDown(Map(k))) return true;
            KeyCode alt = MapAlt(k);
            return alt != KeyCode.None && Input.GetKeyDown(alt);
        }

        public static bool IsHeld(GameKey k)
        {
            if (Input.GetKey(Map(k))) return true;
            KeyCode alt = MapAlt(k);
            return alt != KeyCode.None && Input.GetKey(alt);
        }

        public static Vector2 PointerPosition
        {
            get { return new Vector2(Input.mousePosition.x, Input.mousePosition.y); }
        }

        public static bool PointerPressed
        {
            get { return Input.GetMouseButtonDown(0); }
        }

        public static void Poll()
        {
            string s = Input.inputString;
            if (string.IsNullOrEmpty(s)) return;
            for (int i = 0; i < s.Length; i++)
            {
                if (typed.Count < 64) typed.Add(s[i]);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            typed.Clear();
        }
#endif

        public static string TakeTyped()
        {
            if (typed.Count == 0) return string.Empty;
            string s = new string(typed.ToArray());
            typed.Clear();
            return s;
        }

        public static void ClearTyped()
        {
            typed.Clear();
        }
    }
}

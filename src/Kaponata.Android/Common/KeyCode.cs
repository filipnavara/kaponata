// <copyright file="KeyCode.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Common
{
    /// <summary>
    /// The Android key codes.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/frameworks/native/+/master/include/android/keycodes.h"/>
    public enum KeyCode
    {
        /// <summary>
        /// Unknown key code.
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        ///  Soft Left key. Usually situated below the display on phones and used as a multi-function
        ///  feature key for selecting a software defined function shown on the bottom left of the display.
        /// </summary>
        SOFT_LEFT = 1,

        /// <summary>
        /// Soft Right key. Usually situated below the display on phones and used as a multi-function
        /// feature key for selecting a software defined function shown on the bottom right of the display.
        /// </summary>
        SOFT_RIGHT = 2,

        /// <summary>
        /// Home key. This key is handled by the framework and is never delivered to applications.
        /// </summary>
        HOME = 3,

        /// <summary>
        /// Back key.
        /// </summary>
        BACK = 4,

        /// <summary>
        /// Call key.
        /// </summary>
        CALL = 5,

        /// <summary>
        /// End Call key.
        /// </summary>
        ENDCALL = 6,

        /// <summary>
        /// The '0' key.
        /// </summary>
        D0 = 7,

        /// <summary>
        /// The '1' key.
        /// </summary>
        D1 = 8,

        /// <summary>
        /// The '2' key.
        /// </summary>
        D2 = 9,

        /// <summary>
        /// The '3' key.
        /// </summary>
        D3 = 10,

        /// <summary>
        /// The '4' key.
        /// </summary>
        D4 = 11,

        /// <summary>
        /// The '5' key.
        /// </summary>
        D5 = 12,

        /// <summary>
        /// The '6' key.
        /// </summary>
        D6 = 13,

        /// <summary>
        /// The '7' key.
        /// </summary>
        D7 = 14,

        /// <summary>
        /// The '8' key.
        /// </summary>
        D8 = 15,

        /// <summary>
        /// The '9' key.
        /// </summary>
        D9 = 16,

        /// <summary>
        /// '*' key.
        /// </summary>
        STAR = 17,

        /// <summary>
        /// '#' key.
        /// </summary>
        POUND = 18,

        /// <summary>
        /// Directional Pad Up key. May also be synthesized from trackball motions.
        /// </summary>
        DPAD_UP = 19,

        /// <summary>
        /// Directional Pad Down key. May also be synthesized from trackball motions.
        /// </summary>
        DPAD_DOWN = 20,

        /// <summary>
        /// Directional Pad Left key. May also be synthesized from trackball motions.
        /// </summary>
        DPAD_LEFT = 21,

        /// <summary>
        /// Directional Pad Right key. May also be synthesized from trackball motions.
        /// </summary>
        DPAD_RIGHT = 22,

        /// <summary>
        /// Directional Pad Center key. May also be synthesized from trackball motions.
        /// </summary>
        DPAD_CENTER = 23,

        /// <summary>
        /// Volume Up key. Adjusts the speaker volume up.
        /// </summary>
        VOLUME_UP = 24,

        /// <summary>
        /// Volume Down key. Adjusts the speaker volume down.
        /// </summary>
        VOLUME_DOWN = 25,

        /// <summary>
        /// Power key.
        /// </summary>
        POWER = 26,

        /// <summary>
        /// Camera key. Used to launch a camera application or take pictures.
        /// </summary>
        CAMERA = 27,

        /// <summary>
        /// Clear key.
        /// </summary>
        CLEAR = 28,

        /// <summary>
        /// 'A' key.
        /// </summary>
        A = 29,

        /// <summary>
        /// 'B' key.
        /// </summary>
        B = 30,

        /// <summary>
        /// 'C' key.
        /// </summary>
        C = 31,

        /// <summary>
        /// 'D' key.
        /// </summary>
        D = 32,

        /// <summary>
        /// 'E' key.
        /// </summary>
        E = 33,

        /// <summary>
        /// 'F' key.
        /// </summary>
        F = 34,

        /// <summary>
        /// 'G' key.
        /// </summary>
        G = 35,

        /// <summary>
        /// 'H' key.
        /// </summary>
        H = 36,

        /// <summary>
        /// 'I' key.
        /// </summary>
        I = 37,

        /// <summary>
        /// 'J' key.
        /// </summary>
        J = 38,

        /// <summary>
        /// 'K' key.
        /// </summary>
        K = 39,

        /// <summary>
        /// 'L' key.
        /// </summary>
        L = 40,

        /// <summary>
        /// 'M' key.
        /// </summary>
        M = 41,

        /// <summary>
        /// 'N' key.
        /// </summary>
        N = 42,

        /// <summary>
        /// 'O' key.
        /// </summary>
        O = 43,

        /// <summary>
        /// 'P' key.
        /// </summary>
        P = 44,

        /// <summary>
        /// 'Q' key.
        /// </summary>
        Q = 45,

        /// <summary>
        /// 'R' key.
        /// </summary>
        R = 46,

        /// <summary>
        /// 'S' key.
        /// </summary>
        S = 47,

        /// <summary>
        /// 'T' key.
        /// </summary>
        T = 48,

        /// <summary>
        /// 'U' key.
        /// </summary>
        U = 49,

        /// <summary>
        /// 'V' key.
        /// </summary>
        V = 50,

        /// <summary>
        /// 'W' key.
        /// </summary>
        W = 51,

        /// <summary>
        /// 'X' key.
        /// </summary>
        X = 52,

        /// <summary>
        /// 'Y' key.
        /// </summary>
        Y = 53,

        /// <summary>
        /// 'Z' key.
        /// </summary>
        Z = 54,

        /// <summary>
        /// ',' key.
        /// </summary>
        COMMA = 55,

        /// <summary>
        /// '.' key.
        /// </summary>
        PERIOD = 56,

        /// <summary>
        /// Left Alt modifier key.
        /// </summary>
        ALT_LEFT = 57,

        /// <summary>
        /// Right Alt modifier key.
        /// </summary>
        ALT_RIGHT = 58,

        /// <summary>
        /// Left Shift modifier key.
        /// </summary>
        SHIFT_LEFT = 59,

        /// <summary>
        /// Right Shift modifier key.
        /// </summary>
        SHIFT_RIGHT = 60,

        /// <summary>
        /// Tab key.
        /// </summary>
        TAB = 61,

        /// <summary>
        /// Space key.
        /// </summary>
        SPACE = 62,

        /// <summary>
        /// Symbol modifier key. Used to enter alternate symbols.
        /// </summary>
        SYM = 63,

        /// <summary>
        /// Explorer special function key. Used to launch a browser application.
        /// </summary>
        EXPLORER = 64,

        /// <summary>
        /// Envelope special function key. Used to launch a mail application.
        /// </summary>
        ENVELOPE = 65,

        /// <summary>
        /// Enter key.
        /// </summary>
        ENTER = 66,

        /// <summary>
        /// Backspace key. Deletes characters before the insertion point, unlike <see cref="FORWARD_DEL"/>.
        /// </summary>
        DEL = 67,

        /// <summary>
        /// '`' (backtick) key.
        /// </summary>
        GRAVE = 68,

        /// <summary>
        ///  '-'.(minus) key.
        /// </summary>
        MINUS = 69,

        /// <summary>
        ///  '=' key.
        /// </summary>
        EQUAL = 70,

        /// <summary>
        /// '[' key.
        /// </summary>
        LEFT_BRACKET = 71,

        /// <summary>
        /// ']' key.
        /// </summary>
        RIGHT_BRACKET = 72,

        /// <summary>
        ///  '\' key.
        /// </summary>
        BACKSLASH = 73,

        /// <summary>
        /// ';' key.
        /// </summary>
        SEMICOLON = 74,

        /// <summary>
        /// ''' (apostrophe) key.
        /// </summary>
        APOSTROPHE = 75,

        /// <summary>
        /// '/' key.
        /// </summary>
        SLASH = 76,

        /// <summary>
        /// '@' key.
        /// </summary>
        AT = 77,

        /// <summary>
        /// Number modifier key. Used to enter numeric symbols. This key is not Num Lock; it is more like
        /// <see cref="ALT_LEFT"/> and is interpreted as an ALT key by <c>MetaKeyKeyListener</c>.
        /// </summary>
        NUM = 78,

        /// <summary>
        ///  Headset Hook key. Used to hang up calls and stop media.
        /// </summary>
        HEADSETHOOK = 79,

        /// <summary>
        /// Camera Focus key. Used to focus the camera.
        /// </summary>
        FOCUS = 80,

        /// <summary>
        ///  '+' key.
        /// </summary>
        PLUS = 81,

        /// <summary>
        /// Menu key.
        /// </summary>
        MENU = 82,

        /// <summary>
        /// Notification key.
        /// </summary>
        NOTIFICATION = 83,

        /// <summary>
        /// Search key.
        /// </summary>
        SEARCH = 84,

        /// <summary>
        /// Play/Pause media key.
        /// </summary>
        MEDIA_PLAY_PAUSE = 85,

        /// <summary>
        /// Stop media key.
        /// </summary>
        MEDIA_STOP = 86,

        /// <summary>
        /// Play Next media key.
        /// </summary>
        MEDIA_NEXT = 87,

        /// <summary>
        /// Play Previous media key.
        /// </summary>
        MEDIA_PREVIOUS = 88,

        /// <summary>
        /// Rewind media key.
        /// </summary>
        MEDIA_REWIND = 89,

        /// <summary>
        /// Fast Forward media key.
        /// </summary>
        MEDIA_FAST_FORWARD = 90,

        /// <summary>
        /// ute key. Mutes the microphone, unlike <see cref="VOLUME_MUTE"/>.
        /// </summary>
        MUTE = 91,

        /// <summary>
        /// Page Up key.
        /// </summary>
        PAGE_UP = 92,

        /// <summary>
        /// Page Down key.
        /// </summary>
        PAGE_DOWN = 93,

        /// <summary>
        /// Picture Symbols modifier key. Used to switch symbol sets (Emoji, Kao-moji).
        /// </summary>
        PICTSYMBOLS = 94,

        /// <summary>
        /// Switch Charset modifier key. Used to switch character sets (Kanji, Katakana).
        /// </summary>
        SWITCH_CHARSET = 95,

        /// <summary>
        /// A Button key. On a game controller, the A button should be either the button labeled A or
        /// the first button on the bottom row of controller buttons.
        /// </summary>
        BUTTON_A = 96,

        /// <summary>
        /// B Button key. On a game controller, the B button should be either the button labeled B or
        /// the second button on the bottom row of controller buttons.
        /// </summary>
        BUTTON_B = 97,

        /// <summary>
        /// C Button key. On a game controller, the C button should be either the button labeled C or
        /// the third button on the bottom row of controller buttons.
        /// </summary>
        BUTTON_C = 98,

        /// <summary>
        /// X Button key. On a game controller, the X button should be either the button labeled X
        /// or the first button on the upper row of controller buttons.
        /// </summary>
        BUTTON_X = 99,

        /// <summary>
        /// Y Button key. On a game controller, the Y button should be either the button labeled Y
        /// or the second button on the upper row of controller buttons.
        /// </summary>
        BUTTON_Y = 100,

        /// <summary>
        /// Z Button key. On a game controller, the Z button should be either the button labeled Z
        /// or the third button on the upper row of controller buttons.
        /// </summary>
        BUTTON_Z = 101,

        /// <summary>
        /// L1 Button key. On a game controller, the L1 button should be either the button labeled
        /// L1 (or L) or the top left trigger button.
        /// </summary>
        BUTTON_L1 = 102,

        /// <summary>
        /// R1 Button key. On a game controller, the R1 button should be either the button labeled
        /// R1 (or R) or the top right trigger button.
        /// </summary>
        BUTTON_R1 = 103,

        /// <summary>
        ///  L2 Button key. On a game controller, the L2 button should be either the button labeled
        ///  L2 or the bottom left trigger button.
        /// </summary>
        BUTTON_L2 = 104,

        /// <summary>
        /// R2 Button key. On a game controller, the R2 button should be either the button labeled
        /// R2 or the bottom right trigger button.
        /// </summary>
        BUTTON_R2 = 105,

        /// <summary>
        /// Left Thumb Button key. On a game controller, the left thumb button indicates that the left
        /// (or only) joystick is pressed.
        /// </summary>
        BUTTON_THUMBL = 106,

        /// <summary>
        /// Right Thumb Button key. On a game controller, the right thumb button indicates that the
        /// right joystick is pressed.
        /// </summary>
        BUTTON_THUMBR = 107,

        /// <summary>
        /// Start Button key. On a game controller, the button labeled Start.
        /// </summary>
        BUTTON_START = 108,

        /// <summary>
        /// Select Button key. On a game controller, the button labeled Select.
        /// </summary>
        BUTTON_SELECT = 109,

        /// <summary>
        /// Mode Button key. On a game controller, the button labeled Mode.
        /// </summary>
        BUTTON_MODE = 110,

        /// <summary>
        /// Escape key.
        /// </summary>
        ESCAPE = 111,

        /// <summary>
        /// Forward Delete key. Deletes characters ahead of the insertion point, unlike <see cref="DEL"/>.
        /// </summary>
        FORWARD_DEL = 112,

        /// <summary>
        /// Left Control modifier key.
        /// </summary>
        CTRL_LEFT = 113,

        /// <summary>
        /// Right Control modifier key.
        /// </summary>
        CTRL_RIGHT = 114,

        /// <summary>
        /// Caps Lock key.
        /// </summary>
        CAPS_LOCK = 115,

        /// <summary>
        /// Scroll Lock key.
        /// </summary>
        SCROLL_LOCK = 116,

        /// <summary>
        /// eft Meta modifier key.
        /// </summary>
        META_LEFT = 117,

        /// <summary>
        /// Right Meta modifier key.
        /// </summary>
        META_RIGHT = 118,

        /// <summary>
        /// Function modifier key.
        /// </summary>
        FUNCTION = 119,

        /// <summary>
        /// System Request / Print Screen key.
        /// </summary>
        SYSRQ = 120,

        /// <summary>
        /// Break / Pause key.
        /// </summary>
        BREAK = 121,

        /// <summary>
        /// Home Movement key. Used for scrolling or moving the cursor around to the start
        /// of a line or to the top of a list.
        /// </summary>
        MOVE_HOME = 122,

        /// <summary>
        /// End Movement key. Used for scrolling or moving the cursor around to the end of a line
        /// or to the bottom of a list.
        /// </summary>
        MOVE_END = 123,

        /// <summary>
        /// Insert key. Toggles insert / overwrite edit mode.
        /// </summary>
        INSERT = 124,

        /// <summary>
        /// Forward key. Navigates forward in the history stack. Complement of <see cref="BACK"/>.
        /// </summary>
        FORWARD = 125,

        /// <summary>
        /// Play media key.
        /// </summary>
        MEDIA_PLAY = 126,

        /// <summary>
        /// Pause media key.
        /// </summary>
        MEDIA_PAUSE = 127,

        /// <summary>
        /// Close media key. May be used to close a CD tray, for example.
        /// </summary>
        MEDIA_CLOSE = 128,

        /// <summary>
        /// Eject media key. May be used to eject a CD tray, for example.
        /// </summary>
        MEDIA_EJECT = 129,

        /// <summary>
        /// Record media key.
        /// </summary>
        MEDIA_RECORD = 130,

        /// <summary>
        /// F1 key.
        /// </summary>
        F1 = 131,

        /// <summary>
        /// F2 key.
        /// </summary>
        F2 = 132,

        /// <summary>
        /// F3 key.
        /// </summary>
        F3 = 133,

        /// <summary>
        /// F4 key.
        /// </summary>
        F4 = 134,

        /// <summary>
        /// F5 key.
        /// </summary>
        F5 = 135,

        /// <summary>
        /// F6 key.
        /// </summary>
        F6 = 136,

        /// <summary>
        /// F7 key.
        /// </summary>
        F7 = 137,

        /// <summary>
        /// F8 key.
        /// </summary>
        F8 = 138,

        /// <summary>
        /// F9 key.
        /// </summary>
        F9 = 139,

        /// <summary>
        /// F10 key.
        /// </summary>
        F10 = 140,

        /// <summary>
        /// F11 key.
        /// </summary>
        F11 = 141,

        /// <summary>
        /// F12 key.
        /// </summary>
        F12 = 142,

        /// <summary>
        /// Num Lock key. This is the Num Lock key; it is different from <see cref="NUM"/>. This key alters the
        /// behavior of other keys on the numeric keypad.
        /// </summary>
        NUM_LOCK = 143,

        /// <summary>
        /// Numeric keypad '0' key.
        /// </summary>
        NUMPAD_0 = 144,

        /// <summary>
        /// Numeric keypad '1' key.
        /// </summary>
        NUMPAD_1 = 145,

        /// <summary>
        /// Numeric keypad '2' key.
        /// </summary>
        NUMPAD_2 = 146,

        /// <summary>
        /// Numeric keypad '3' key.
        /// </summary>
        NUMPAD_3 = 147,

        /// <summary>
        /// Numeric keypad '4' key.
        /// </summary>
        NUMPAD_4 = 148,

        /// <summary>
        /// Numeric keypad '5' key.
        /// </summary>
        NUMPAD_5 = 149,

        /// <summary>
        /// Numeric keypad '6' key.
        /// </summary>
        NUMPAD_6 = 150,

        /// <summary>
        /// Numeric keypad '7' key.
        /// </summary>
        NUMPAD_7 = 151,

        /// <summary>
        /// Numeric keypad '8' key.
        /// </summary>
        NUMPAD_8 = 152,

        /// <summary>
        /// Numeric keypad '9' key.
        /// </summary>
        NUMPAD_9 = 153,

        /// <summary>
        /// Numeric keypad '/' key (for division).
        /// </summary>
        NUMPAD_DIVIDE = 154,

        /// <summary>
        /// Numeric keypad '*' key (for multiplication).
        /// </summary>
        NUMPAD_MULTIPLY = 155,

        /// <summary>
        /// Numeric keypad '-' key (for subtraction).
        /// </summary>
        NUMPAD_SUBTRACT = 156,

        /// <summary>
        /// Numeric keypad '+' key (for addition).
        /// </summary>
        NUMPAD_ADD = 157,

        /// <summary>
        /// Numeric keypad '.' key (for decimals or digit grouping).
        /// </summary>
        NUMPAD_DOT = 158,

        /// <summary>
        /// Numeric keypad ',' key (for decimals or digit grouping).
        /// </summary>
        NUMPAD_COMMA = 159,

        /// <summary>
        /// Numeric keypad Enter key.
        /// </summary>
        NUMPAD_ENTER = 160,

        /// <summary>
        /// Numeric keypad '=' key.
        /// </summary>
        NUMPAD_EQUALS = 161,

        /// <summary>
        /// Numeric keypad '(' key.
        /// </summary>
        NUMPAD_LEFT_PAREN = 162,

        /// <summary>
        /// Numeric keypad ')' key.
        /// </summary>
        NUMPAD_RIGHT_PAREN = 163,

        /// <summary>
        /// Volume Mute key. Mutes the speaker, unlike <see cref="MUTE"/>. This key should normally be
        /// implemented as a toggle such that the first press mutes the speaker and the second press
        /// restores the original volume.
        /// </summary>
        VOLUME_MUTE = 164,

        /// <summary>
        /// Info key. Common on TV remotes to show additional information related to what is currently
        /// being viewed.
        /// </summary>
        INFO = 165,

        /// <summary>
        /// Channel up key. On TV remotes, increments the television channel.
        /// </summary>
        CHANNEL_UP = 166,

        /// <summary>
        /// Channel down key. On TV remotes, decrements the television channel.
        /// </summary>
        CHANNEL_DOWN = 167,

        /// <summary>
        /// Zoom in key.
        /// </summary>
        ZOOM_IN = 168,

        /// <summary>
        /// Zoom out key.
        /// </summary>
        ZOOM_OUT = 169,

        /// <summary>
        /// TV key. On TV remotes, switches to viewing live TV.
        /// </summary>
        TV = 170,

        /// <summary>
        /// Window key. On TV remotes, toggles picture-in-picture mode or other windowing functions.
        /// </summary>
        WINDOW = 171,

        /// <summary>
        /// Guide key. On TV remotes, shows a programming guide.
        /// </summary>
        GUIDE = 172,

        /// <summary>
        /// DVR key. On some TV remotes, switches to a DVR mode for recorded shows.
        /// </summary>
        DVR = 173,

        /// <summary>
        /// Bookmark key. On some TV remotes, bookmarks content or web pages.
        /// </summary>
        BOOKMARK = 174,

        /// <summary>
        /// Toggle captions key. Switches the mode for closed-captioning text, for example during
        /// television shows.
        /// </summary>
        CAPTIONS = 175,

        /// <summary>
        /// Settings key. Starts the system settings activity.
        /// </summary>
        SETTINGS = 176,

        /// <summary>
        /// TV power key. On TV remotes, toggles the power on a television screen.
        /// </summary>
        TV_POWER = 177,

        /// <summary>
        /// TV input key. On TV remotes, switches the input on a television screen.
        /// </summary>
        TV_INPUT = 178,

        /// <summary>
        /// Set-top-box power key. On TV remotes, toggles the power on an external Set-top-box.
        /// </summary>
        STB_POWER = 179,

        /// <summary>
        /// Set-top-box input key. On TV remotes, switches the input mode on an external Set-top-box.
        /// </summary>
        STB_INPUT = 180,

        /// <summary>
        /// A/V Receiver power key. On TV remotes, toggles the power on an external A/V Receiver.
        /// </summary>
        AVR_POWER = 181,

        /// <summary>
        /// A/V Receiver input key. On TV remotes, switches the input mode on an external A/V Receiver.
        /// </summary>
        AVR_INPUT = 182,

        /// <summary>
        /// Red "programmable" key. On TV remotes, acts as a contextual/programmable key.
        /// </summary>
        PROG_RED = 183,

        /// <summary>
        /// Green "programmable" key. On TV remotes, actsas a contextual/programmable key.
        /// </summary>
        PROG_GREEN = 184,

        /// <summary>
        /// Yellow "programmable" key. On TV remotes, acts as a contextual/programmable key.
        /// </summary>
        PROG_YELLOW = 185,

        /// <summary>
        /// Blue "programmable" key. On TV remotes, acts as a contextual/programmable key.
        /// </summary>
        PROG_BLUE = 186,

        /// <summary>
        /// App switch key. Should bring up the application switcher dialog.
        /// </summary>
        APP_SWITCH = 187,

        /// <summary>
        /// Generic Game Pad Button #1.
        /// </summary>
        BUTTON_1 = 188,

        /// <summary>
        /// Generic Game Pad Button #2.
        /// </summary>
        BUTTON_2 = 189,

        /// <summary>
        /// Generic Game Pad Button #3.
        /// </summary>
        BUTTON_3 = 190,

        /// <summary>
        /// Generic Game Pad Button #4.
        /// </summary>
        BUTTON_4 = 191,

        /// <summary>
        /// Generic Game Pad Button #5.
        /// </summary>
        BUTTON_5 = 192,

        /// <summary>
        /// Generic Game Pad Button #6.
        /// </summary>
        BUTTON_6 = 193,

        /// <summary>
        /// Generic Game Pad Button #7.
        /// </summary>
        BUTTON_7 = 194,

        /// <summary>
        /// Generic Game Pad Button #8.
        /// </summary>
        BUTTON_8 = 195,

        /// <summary>
        /// Generic Game Pad Button #9.
        /// </summary>
        BUTTON_9 = 196,

        /// <summary>
        /// Generic Game Pad Button #10.
        /// </summary>
        BUTTON_10 = 197,

        /// <summary>
        /// Generic Game Pad Button #11.
        /// </summary>
        BUTTON_11 = 198,

        /// <summary>
        /// Generic Game Pad Button #12.
        /// </summary>
        BUTTON_12 = 199,

        /// <summary>
        /// Generic Game Pad Button #13.
        /// </summary>
        BUTTON_13 = 200,

        /// <summary>
        /// Generic Game Pad Button #14.
        /// </summary>
        BUTTON_14 = 201,

        /// <summary>
        /// Generic Game Pad Button #15.
        /// </summary>
        BUTTON_15 = 202,

        /// <summary>
        /// Generic Game Pad Button #16.
        /// </summary>
        BUTTON_16 = 203,

        /// <summary>
        /// Language Switch key.
        /// Toggles the current input language such as switching between English and Japanese on
        /// a QWERTY keyboard.  On some devices, the same function may be performed by
        /// pressing Shift+Spacebar.
        /// </summary>
        LANGUAGE_SWITCH = 204,

        /// <summary>
        /// Manner Mode key.
        /// Toggles silent or vibrate mode on and off to make the device behave more politely
        /// in certain settings such as on a crowded train.  On some devices, the key may only
        /// operate when long-pressed.
        /// </summary>
        MANNER_MODE = 205,

        /// <summary>
        /// 3D Mode key.
        /// Toggles the display between 2D and 3D mode. */
        /// </summary>
        _3D_MODE = 206,

        /// <summary>
        /// Contacts special function key.
        /// Used to launch an address book application.
        /// </summary>
        CONTACTS = 207,

        /// <summary>
        /// Calendar special function key.
        /// Used to launch a calendar application.
        /// </summary>
        CALENDAR = 208,

        /// <summary>
        /// Music special function key.
        /// Used to launch a music player application.
        /// </summary>
        MUSIC = 209,

        /// <summary>
        /// Calculator special function key.
        /// Used to launch a calculator application.
        /// </summary>
        CALCULATOR = 210,

        /// <summary>
        /// Japanese full-width / half-width key.
        /// </summary>
        ZENKAKU_HANKAKU = 211,

        /// <summary>
        /// Japanese alphanumeric key.
        /// </summary>
        EISU = 212,

        /// <summary>
        /// Japanese non-conversion key.
        /// </summary>
        MUHENKAN = 213,

        /// <summary>
        /// Japanese conversion key.
        /// </summary>
        HENKAN = 214,

        /// <summary>
        /// Japanese katakana / hiragana key.
        /// </summary>
        KATAKANA_HIRAGANA = 215,

        /// <summary>
        /// Japanese Yen key.
        /// </summary>
        YEN = 216,

        /// <summary>
        /// Japanese Ro key.
        /// </summary>
        RO = 217,

        /// <summary>
        /// Japanese kana key.
        /// </summary>
        KANA = 218,

        /// <summary>
        /// Assist key.
        /// Launches the global assist activity.  Not delivered to applications.
        /// </summary>
        ASSIST = 219,

        /// <summary>
        /// Brightness Down key.
        /// Adjusts the screen brightness down.
        /// </summary>
        BRIGHTNESS_DOWN = 220,

        /// <summary>
        /// Brightness Up key.
        /// Adjusts the screen brightness up.
        /// </summary>
        BRIGHTNESS_UP = 221,

        /// <summary>
        /// Audio Track key.
        /// Switches the audio tracks. */
        /// </summary>
        MEDIA_AUDIO_TRACK = 222,

        /// <summary>
        /// Sleep key.
        /// Puts the device to sleep.  Behaves somewhat like <see cref="POWER"/> but it
        /// has no effect if the device is already asleep.
        /// </summary>
        SLEEP = 223,

        /// <summary>
        /// Wakeup key.
        /// Wakes up the device.  Behaves somewhat like <see cref="POWER"/> but it
        /// has no effect if the device is already awake.
        /// </summary>
        WAKEUP = 224,

        /// <summary>
        /// Pairing key.
        /// Initiates peripheral pairing mode. Useful for pairing remote control
        /// devices or game controllers, especially if no other input mode is
        /// available.
        /// </summary>
        PAIRING = 225,

        /// <summary>
        /// Media Top Menu key.
        /// Goes to the top of media menu.
        /// </summary>
        MEDIA_TOP_MENU = 226,

        /// <summary>
        /// '11' key
        /// </summary>
        D11 = 227,

        /// <summary>
        /// '12' key
        /// </summary>
        D12 = 228,

        /// <summary>
        /// Last Channel key.
        /// Goes to the last viewed channel.
        /// </summary>
        LAST_CHANNEL = 229,

        /// <summary>
        /// TV data service key.
        /// Displays data services like weather, sports.
        /// </summary>
        TV_DATA_SERVICE = 230,

        /// <summary>
        /// Voice Assist key.
        /// Launches the global voice assist activity. Not delivered to applications.
        /// </summary>
        VOICE_ASSIST = 231,

        /// <summary>
        /// Radio key.
        /// Toggles TV service / Radio service.
        /// </summary>
        TV_RADIO_SERVICE = 232,

        /// <summary>
        /// Teletext key.
        /// Displays Teletext service.
        /// </summary>
        TV_TELETEXT = 233,

        /// <summary>
        /// Number entry key.
        /// Initiates to enter multi-digit channel nubmber when each digit key is assigned
        /// for selecting separate channel. Corresponds to Number Entry Mode (0x1D) of CEC
        /// User Control Code.
        /// </summary>
        TV_NUMBER_ENTRY = 234,

        /// <summary>
        /// Analog Terrestrial key.
        /// Switches to analog terrestrial broadcast service.
        /// </summary>
        TV_TERRESTRIAL_ANALOG = 235,

        /// <summary>
        /// Digital Terrestrial key.
        /// Switches to digital terrestrial broadcast service.
        /// </summary>
        TV_TERRESTRIAL_DIGITAL = 236,

        /// <summary>
        /// Satellite key.
        /// Switches to digital satellite broadcast service.
        /// </summary>
        TV_SATELLITE = 237,

        /// <summary>
        /// BS key.
        /// Switches to BS digital satellite broadcasting service available in Japan.
        /// </summary>
        TV_SATELLITE_BS = 238,

        /// <summary>
        /// CS key.
        /// Switches to CS digital satellite broadcasting service available in Japan.
        /// </summary>
        TV_SATELLITE_CS = 239,

        /// <summary>
        /// BS/CS key.
        /// Toggles between BS and CS digital satellite services.
        /// </summary>
        TV_SATELLITE_SERVICE = 240,

        /// <summary>
        /// Toggle Network key.
        /// Toggles selecting broacast services.
        /// </summary>
        TV_NETWORK = 241,

        /// <summary>
        /// Antenna/Cable key.
        /// Toggles broadcast input source between antenna and cable.
        /// </summary>
        TV_ANTENNA_CABLE = 242,

        /// <summary>
        /// HDMI #1 key.
        /// Switches to HDMI input #1.
        /// </summary>
        TV_INPUT_HDMI_1 = 243,

        /// <summary>
        /// HDMI #2 key.
        /// Switches to HDMI input #2.
        /// </summary>
        TV_INPUT_HDMI_2 = 244,

        /// <summary>
        /// HDMI #3 key.
        /// Switches to HDMI input #3.
        /// </summary>
        TV_INPUT_HDMI_3 = 245,

        /// <summary>
        /// HDMI #4 key.
        /// Switches to HDMI input #4.
        /// </summary>
        TV_INPUT_HDMI_4 = 246,

        /// <summary>
        /// Composite #1 key.
        /// Switches to composite video input #1.
        /// </summary>
        TV_INPUT_COMPOSITE_1 = 247,

        /// <summary>
        /// Composite #2 key.
        /// Switches to composite video input #2.
        /// </summary>
        TV_INPUT_COMPOSITE_2 = 248,

        /// <summary>
        /// Component #1 key.
        /// Switches to component video input #1.
        /// </summary>
        TV_INPUT_COMPONENT_1 = 249,

        /// <summary>
        /// Component #2 key.
        /// Switches to component video input #2.
        /// </summary>
        TV_INPUT_COMPONENT_2 = 250,

        /// <summary>
        /// VGA #1 key.
        /// Switches to VGA (analog RGB) input #1.
        /// </summary>
        TV_INPUT_VGA_1 = 251,

        /// <summary>
        /// Audio description key.
        /// Toggles audio description off / on.
        /// </summary>
        TV_AUDIO_DESCRIPTION = 252,

        /// <summary>
        /// Audio description mixing volume up key.
        /// Louden audio description volume as compared with normal audio volume.
        /// </summary>
        TV_AUDIO_DESCRIPTION_MIX_UP = 253,

        /// <summary>
        /// Audio description mixing volume down key.
        /// Lessen audio description volume as compared with normal audio volume.
        /// </summary>
        TV_AUDIO_DESCRIPTION_MIX_DOWN = 254,

        /// <summary>
        /// Zoom mode key.
        /// Changes Zoom mode (Normal, Full, Zoom, Wide-zoom, etc.)
        /// </summary>
        TV_ZOOM_MODE = 255,

        /// <summary>
        /// Contents menu key.
        /// Goes to the title list. Corresponds to Contents Menu (0x0B) of CEC User Control
        /// Code
        /// </summary>
        TV_CONTENTS_MENU = 256,

        /// <summary>
        /// Media context menu key.
        /// Goes to the context menu of media contents. Corresponds to Media Context-sensitive
        /// Menu (0x11) of CEC User Control Code.
        /// </summary>
        TV_MEDIA_CONTEXT_MENU = 257,

        /// <summary>
        /// Timer programming key.
        /// Goes to the timer recording menu. Corresponds to Timer Programming (0x54) of
        /// CEC User Control Code.
        /// </summary>
        TV_TIMER_PROGRAMMING = 258,

        /// <summary>
        /// Help key.
        /// </summary>
        HELP = 259,

        /// <summary>
        /// Navigate to previous key. Goes backward by one item in an ordered collection of items.
        /// </summary>
        NAVIGATE_PREVIOUS = 260,

        /// <summary>
        /// Advances to the next item in an ordered collection of items.
        /// </summary>
        NAVIGATE_NEXT = 261,

        /// <summary>
        /// Navigate in key. Activates the item that currently has focus or expands to the next level
        /// of a navigation hierarchy.
        /// </summary>
        NAVIGATE_IN = 262,

        /// <summary>
        /// Navigate out key. Backs out one level of a navigation hierarchy or collapses the item that
        /// currently has focus.
        /// </summary>
        NAVIGATE_OUT = 263,

        /// <summary>
        /// Primary stem key for Wear
        /// Main power/reset button on watch.
        /// </summary>
        STEM_PRIMARY = 264,

        /// <summary>
        /// Generic stem key 1 for Wear
        /// </summary>
        STEM_1 = 265,

        /// <summary>
        /// Generic stem key 2 for Wear
        /// </summary>
        STEM_2 = 266,

        /// <summary>
        /// Generic stem key 3 for Wear
        /// </summary>
        STEM_3 = 267,

        /// <summary>
        /// Skip forward media key.
        /// </summary>
        MEDIA_SKIP_FORWARD = 272,

        /// <summary>
        /// Skip backward media key.
        /// </summary>
        MEDIA_SKIP_BACKWARD = 273,

        /// <summary>
        /// Step forward media key. Steps media forward, one frame at a time.
        /// </summary>
        MEDIA_STEP_FORWARD = 274,

        /// <summary>
        /// Step backward media key. Steps media backward, one frame at a time.
        /// </summary>
        MEDIA_STEP_BACKWARD = 275,

        /// <summary>
        /// Put device to sleep unless a wakelock is held.
        /// </summary>
        SOFT_SLEEP = 276,
    }
}

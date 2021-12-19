using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Net
{
    public static class RenderedLinks
    {
        private const string No_Art_Url = "https://i.ibb.co/0BjV95r/NoArt.png";

        private static Dictionary<ushort, string> renderedLinks = new Dictionary<ushort, string>
        {
            { 0x100, "https://i.ibb.co/ypgVM4N/Sword-01.png" }, // Swords
            { 0x101, "https://i.ibb.co/s5B01N4/Sword-02.png" },
            { 0x102, "https://i.ibb.co/f0Vgcbw/Sword-03.png" },
            { 0x103, "https://i.ibb.co/nzjFQdf/Sword-04.png" },
            { 0x104, "https://i.ibb.co/x8PSHbx/Sword-05.png" },
            { 0x105, "https://i.ibb.co/rFTgcSP/Sword-06.png" },
            { 0x106, "https://i.ibb.co/TKLjX0t/Sword-07.png" },
            { 0x107, "https://i.ibb.co/5nRV22K/Sword-08.png" },
            { 0x108, "https://i.ibb.co/M2vkZh5/Sword-09.png" },
            { 0x109, "https://i.ibb.co/6s9K79J/Sword-10.png" },

            { 0x120, "https://i.ibb.co/vB9hF4B/Claymore-01.png" }, // Claymore
            { 0x121, "https://i.ibb.co/KXV62sW/Claymore-02.png" },
            { 0x122, "https://i.ibb.co/fC7WCFw/Claymore-03.png" },
            { 0x123, "https://i.ibb.co/gPzp5GM/Claymore-04.png" },
            { 0x124, "https://i.ibb.co/r47pMYz/Claymore-05.png" },
            { 0x125, "https://i.ibb.co/ys3Lgmd/Claymore-06.png" },
            { 0x126, "https://i.ibb.co/XCL4KTf/Claymore-07.png" },
            { 0x127, "https://i.ibb.co/zrr0LT1/Claymore-08.png" },
            { 0x128, "https://i.ibb.co/pnz4DcQ/Claymore-09.png" },
            { 0x129, "https://i.ibb.co/DzwFvw3/Claymore-10.png" },

            { 0x140, "https://i.ibb.co/0JdsC5Y/Bow-01.png" }, // Bows
            { 0x141, "https://i.ibb.co/18WYWW8/Bow-02.png" },
            { 0x142, "https://i.ibb.co/JqZzT0f/Bow-03.png" },
            { 0x143, "https://i.ibb.co/f2mFdCP/Bow-04.png" },
            { 0x144, "https://i.ibb.co/3NsBkqy/Bow-05.png" },
            { 0x145, "https://i.ibb.co/8zxct76/Bow-06.png" },
            { 0x146, "https://i.ibb.co/wJN3VC5/Bow-07.png" },
            { 0x147, "https://i.ibb.co/grVsBZ5/Bow-08.png" },
            { 0x148, "https://i.ibb.co/wJnPBvT/Bow-09.png" },
            { 0x149, "https://i.ibb.co/QdvMgSQ/Bow-10.png" },

            { 0x160, "https://i.ibb.co/C60hbbS/Spear-01.png" }, // Spears
            { 0x161, "https://i.ibb.co/nbn3cNb/Spear-02.png" },
            { 0x162, "https://i.ibb.co/8ch6GbG/Spear-03.png" },
            { 0x163, "https://i.ibb.co/TR3RjD1/Spear-04.png" },
            { 0x164, "https://i.ibb.co/MRKs2Sj/Spear-05.png" },
            { 0x165, "https://i.ibb.co/C7VGr6W/Spear-06.png" },
            { 0x166, "https://i.ibb.co/d0XxsXZ/Spear-07.png" },
            { 0x167, "https://i.ibb.co/HKYMtG5/Spear-08.png" },
            { 0x168, "https://i.ibb.co/fr0jcbt/Spear-09.png" },
            { 0x169, "https://i.ibb.co/cwg882r/Spear-10.png" },

            { 0x180, "https://i.ibb.co/BPwbNsm/Elixir-01.png" }, // Elixirs
            { 0x181, "https://i.ibb.co/Cw7vmqC/Elixir-02.png" },
            { 0x182, "https://i.ibb.co/Wgwk9xK/Elixir-03.png" },
            { 0x183, "https://i.ibb.co/X7fc3bB/Elixir-04.png" },
            { 0x184, "https://i.ibb.co/fC0W6rW/Elixir-05.png" },
            { 0x185, "https://i.ibb.co/gMkGWTh/Elixir-06.png" },
            { 0x186, "https://i.ibb.co/WDhD3Ds/Elixir-07.png" },
            { 0x187, "https://i.ibb.co/J3KPj89/Elixir-08.png" },
            { 0x188, "https://i.ibb.co/18W1XNc/Elixir-09.png" },
            { 0x189, "https://i.ibb.co/TqYJprm/Elixir-10.png" },

            { 0x240, "https://i.ibb.co/3cgzpTq/Robe-01.png" }, // Robes
            { 0x241, "https://i.ibb.co/QNd6WFS/Robe-02.png" }, 
            { 0x242, "https://i.ibb.co/nmWZW55/Robe-03.png" },
            { 0x243, "https://i.ibb.co/QJQ8HJH/Robe-04.png" },
            { 0x244, "https://i.ibb.co/kHTQPcc/Robe-05.png" },
            { 0x245, "https://i.ibb.co/YysWcHX/Robe-06.png" },
            { 0x246, "https://i.ibb.co/7XJhBtP/Robe-07.png" },
            { 0x247, "https://i.ibb.co/GJN7Mhj/Robe-08.png" },
            { 0x248, "https://i.ibb.co/gFHXvCq/Robe-09.png" },
            { 0x249, "https://i.ibb.co/mC5rQdR/Robe-10.png" },

            { 0x200, "https://i.ibb.co/NCb0qLb/Heavy-Armor-01.png" }, // Heavy Armors
            { 0x201, "https://i.ibb.co/JcqGRrT/Heavy-Armor-02.png" },
            { 0x202, "https://i.ibb.co/ns1XJdq/Heavy-Armor-03.png" },
            { 0x203, "https://i.ibb.co/K6CWN41/Heavy-Armor-04.png" },
            { 0x204, "https://i.ibb.co/hDDsxKs/Heavy-Armor-05.png" },
            { 0x205, "https://i.ibb.co/DbHL9VX/Heavy-Armor-06.png" },
            { 0x206, "https://i.ibb.co/0Z6XwJT/Heavy-Armor-07.png" },
            { 0x207, "https://i.ibb.co/yByYjzf/Heavy-Armor-08.png" },
            { 0x208, "https://i.ibb.co/tDQYM5P/Heavy-Armor-09.png" },
            { 0x209, "https://i.ibb.co/DCtK2mX/Heavy-Armor-10.png" },

            { 0x220, "https://i.ibb.co/4ZYnd8B/Light-Armor-01.png" }, // Light Armors
            { 0x221, "https://i.ibb.co/G0w0mcS/Light-Armor-02.png" },
            { 0x222, "https://i.ibb.co/XFjWYC8/Light-Armor-03.png" },
            { 0x223, "https://i.ibb.co/BrvGTj7/Light-Armor-04.png" },
            { 0x224, "https://i.ibb.co/kxQPpvy/Light-Armor-05.png" },
            { 0x225, "https://i.ibb.co/DfT5yVg/Light-Armor-06.png" },
            { 0x226, "https://i.ibb.co/Z1W57nn/Light-Armor-07.png" },
            { 0x227, "https://i.ibb.co/CWQH3YJ/Light-Armor-08.png" },
            { 0x228, "https://i.ibb.co/vkFRpJ9/Light-Armor-09.png" },
            { 0x229, "https://i.ibb.co/MMBWdK1/Light-Armor-10.png" },

            { 0x280, "https://i.ibb.co/xY2Pvr3/Ring-01.png" }, // Rings
            { 0x281, "https://i.ibb.co/Qr45SHD/Ring-02.png" },
            { 0x282, "https://i.ibb.co/zXQ8c2d/Ring-03.png" },
            { 0x283, "https://i.ibb.co/tQb9x1x/Ring-04.png" },
            { 0x284, "https://i.ibb.co/7JrPFnH/Ring-05.png" },
            { 0x285, "https://i.ibb.co/GM9N0PX/Ring-06.png" },
            { 0x286, "https://i.ibb.co/f415wRm/Ring-07.png" },
            { 0x287, "https://i.ibb.co/fNtNNXd/Ring-08.png" },
            { 0x288, "https://i.ibb.co/0Kzjk23/Ring-09.png" },
            { 0x289, "https://i.ibb.co/HdMKPJG/Ring-10.png" },
            { 0x28a, "https://i.ibb.co/SnMcWDW/Ring-11.png" },
            { 0x28b, "https://i.ibb.co/4TVtr4T/Ring-12.png" },
            { 0x28c, "https://i.ibb.co/nmd9TYw/Ring-13.png" },
            { 0x28d, "https://i.ibb.co/X3ygzht/Ring-14.png" },
            { 0x28e, "https://i.ibb.co/FHsnMs0/Ring-15.png" },
            { 0x28f, "https://i.ibb.co/KGmq9VF/Ring-16.png" },
            { 0x290, "https://i.ibb.co/QDG2tS7/Ring-17.png" },
            { 0x291, "https://i.ibb.co/k4VB11D/Ring-18.png" },
            { 0x292, "https://i.ibb.co/sHfQDmK/Ring-19.png" },
            { 0x293, "https://i.ibb.co/64nd6VQ/Ring-20.png" },
            { 0x294, "https://i.ibb.co/xhJWC6M/Ring-21.png" },
            { 0x295, "https://i.ibb.co/VVmtpYd/Ring-22.png" },
            { 0x296, "https://i.ibb.co/98g03L4/Ring-23.png" },
            { 0x297, "https://i.ibb.co/Sm13NZk/Ring-24.png" }
        };

        public static string GetRenderedImageUrl(ushort type)
        {
            if (!renderedLinks.TryGetValue(type, out var link))
                return No_Art_Url;
            return link;
        }
    }
}

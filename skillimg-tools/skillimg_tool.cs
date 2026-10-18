// skillimg_tool.cs - minimal WZ img editor built on MapleLib_min.dll
// Usage:
//   skillimg_tool read  <imgfile> <nodepath>            print property value
//   skillimg_tool set   <imgfile> <nodepath> <value>    set CompressedInt property, save to <imgfile>
//   skillimg_tool setall <imgfile> <prefix> <value>     set property on every node matching prefix (e.g. skill/15111007/level)
// nodepath examples: skill/15111007/level/1/attackCount
using System;
using System.IO;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

class SkillImgTool
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length < 2) { Usage(); return 1; }
            string mode = args[0];
            string file = args[1];
            if (!File.Exists(file)) { Console.WriteLine("file not found: " + file); return 1; }

            // GMS old encryption ("全球冒险岛(旧)")
            byte[] iv = WzTool.GetIvByMapleVersion(WzMapleVersion.GMS);
            FileStream fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            WzImage img = new WzImage(Path.GetFileName(file), fs, WzMapleVersion.GMS);
            if (!img.ParseImage(true))
            {
                Console.WriteLine("parse failed (wrong encryption? expected 全球冒险岛(旧)/GMS)");
                fs.Close();
                return 2;
            }

                if (mode == "dump")
                {
                    DumpTree(img, 3);
                }
                else if (mode == "children")
                {
                    WzImageProperty container = img.GetFromPath(args[2]);
                    if (container == null) { Console.WriteLine("not found: " + args[2]); }
                    else if (container.WzProperties == null) { Console.WriteLine("leaf: " + args[2] + " = " + container.WzValue); }
                    else { foreach (WzImageProperty c in container.WzProperties) Console.WriteLine(c.Name + " [" + c.PropertyType + "]" + (c.WzValue != null ? " = " + c.WzValue : "")); }
                }
                else if (mode == "read")
                {
                    ReadPath(img, args[2]);
                }
                else if (mode == "dump")
                {
                    DumpTree(img, 3);
                }
                else if (mode == "set")
                {
                    int value = int.Parse(args[3]);
                    SetPath(img, args[2], value);
                    SaveImg(img, fs, file, iv);
                    Console.WriteLine("saved: " + file);
                }
                else if (mode == "setall")
                {
                    string prefix = args[2];
                    int value = int.Parse(args[3]);
                    string attrName = args.Length >= 4 ? args[4] : "attackCount";
                    int count = SetAllLevels(img, prefix, attrName, value);
                    SaveImg(img, fs, file, iv);
                    Console.WriteLine("set " + count + " nodes of '" + attrName + "' to " + value + ", saved: " + file);
                }
                else if (mode == "wzset")
                {
                    string imgName = args[2];
                    string nodepath = args[3];
                    int value = int.Parse(args[4]);
                    int count = WzSet(file, imgName, nodepath, value);
                    Console.WriteLine("wzset done: " + (count > 0 ? "changed " + nodepath + " -> " + value : "nothing changed") + " in " + file);
                }
                else { Usage(); fs.Close(); return 1; }
                fs.Close();
                return 0;
            }
            catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
            return 3;
        }
    }

    // Save the image the same way HaRepacker does for a standalone .img:
    // mark Changed, write with WzBinaryWriter + IV to a .tmp, then replace.
    static void SaveImg(WzImage img, FileStream fs, string file, byte[] iv)
    {
        img.Changed = true;
        // PNG raw bytes are read lazily during save (GetCompressedBytes reads from fs),
        // so fs must stay open until SaveImage finishes.
        string tmpFilePath = file + ".tmp";
        using (FileStream oldfs = File.Create(tmpFilePath))
        {
            using (WzBinaryWriter wzWriter = new WzBinaryWriter(oldfs, iv))
            {
                img.SaveImage(wzWriter, true);
            }
        }
        fs.Close();
        File.Copy(tmpFilePath, file, true);
        File.Delete(tmpFilePath);
    }

    static void DumpTree(WzImage img, int maxDepth)
    {
        if (img.WzProperties == null) return;
        foreach (WzImageProperty child in img.WzProperties)
        {
            Console.WriteLine(child.Name + " [" + child.PropertyType + "]");
            DumpTree(child, 1, maxDepth);
        }
    }

    static void DumpTree(WzImageProperty prop, int depth, int maxDepth)
    {
        if (prop == null || depth > maxDepth) return;
        if (prop.WzProperties == null) return;
        foreach (WzImageProperty child in prop.WzProperties)
        {
            Console.WriteLine(new string(' ', depth * 2) + child.Name + " [" + child.PropertyType + "]");
            DumpTree(child, depth + 1, maxDepth);
        }
    }

    static void ReadPath(WzImage img, string path)
    {
        WzImageProperty prop = img.GetFromPath(path);
        if (prop == null) { Console.WriteLine("not found: " + path); return; }
        Console.WriteLine(path + " = " + prop.WzValue + " (" + prop.PropertyType + ")");
    }

    static void SetPath(WzImage img, string path, int value)
    {
        WzImageProperty prop = img.GetFromPath(path);
        if (prop == null) { Console.WriteLine("not found: " + path); return; }
        WzIntProperty ci = prop as WzIntProperty;
        if (ci != null) { ci.Value = value; Console.WriteLine(path + " -> " + value); return; }
        Console.WriteLine("not an int property: " + prop.PropertyType);
    }

    // prefix like "skill/15111007/level" - ensures <prefix>/N/<attrName> = value for every level N
    static int SetAllLevels(WzImage img, string prefix, string attrName, int value)
    {
        int count = 0;
        WzImageProperty levelContainer = img.GetFromPath(prefix);
        if (levelContainer == null) { Console.WriteLine("not found: " + prefix); return 0; }
        foreach (WzImageProperty lvl in levelContainer.WzProperties)
        {
            WzImageProperty ac = lvl[attrName];
            WzIntProperty ci = ac as WzIntProperty;
            if (ci != null) { ci.Value = value; count++; continue; }
            if (ac == null)
            {
                WzSubProperty sub = lvl as WzSubProperty;
                if (sub != null)
                {
                    sub.AddProperty(new WzIntProperty(attrName, value));
                    count++;
                    continue;
                }
            }
            Console.WriteLine("  skipped level (no '" + attrName + "' and not a subproperty): " + lvl.Name + " -> " + (ac == null ? "null" : ac.PropertyType.ToString()));
        }
        return count;
    }

    // Open a .wz container, change one int property inside <imgName>, save the whole container.
    // Auto-backup the original file to <wzfile>.bak2 before overwriting.
    static int WzSet(string wzPath, string imgName, string nodepath, int value)
    {
        WzFile wzf = new WzFile(wzPath, WzMapleVersion.GMS);
        WzFileParseStatus st = wzf.ParseWzFile();
        if (st != WzFileParseStatus.Success)
        {
            Console.WriteLine("wz parse failed: " + st);
            return -1;
        }

        // internal WzDirectory wzDir is not visible cross-assembly; reach it via reflection.
        System.Reflection.FieldInfo f = typeof(WzFile).GetField("wzDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        WzDirectory root = f != null ? (WzDirectory)f.GetValue(wzf) : null;
        if (root == null) { Console.WriteLine("cannot access wz root directory"); return -1; }

        WzImage img = null;
        foreach (WzImage im in root.WzImages)
        {
            if (im.Name.Equals(imgName, StringComparison.OrdinalIgnoreCase)) { img = im; break; }
        }
        if (img == null) { Console.WriteLine("img not found in wz: " + imgName); return -1; }

        if (!img.ParseImage(true)) { Console.WriteLine("img parse failed: " + imgName); return -1; }

        WzImageProperty prop = img.GetFromPath(nodepath);
        WzIntProperty ci = prop as WzIntProperty;
        if (ci == null) { Console.WriteLine("not found or not an int property: " + nodepath); return -1; }
        ci.Value = value;

        string backup = wzPath + ".bak2";
        if (!File.Exists(backup)) File.Copy(wzPath, backup);

        // SaveToDisk creates a ".TEMP" file relative to the current directory; run from the wz folder.
        string cur = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Path.GetDirectoryName(wzPath));
        try { wzf.SaveToDisk(wzPath); }
        finally { Directory.SetCurrentDirectory(cur); }
        return 1;
    }

    static void Usage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  skillimg_tool read    <imgfile> <nodepath>");
        Console.WriteLine("  skillimg_tool set     <imgfile> <nodepath> <value>");
        Console.WriteLine("  skillimg_tool setall  <imgfile> <prefix> <value> [attrname]");
        Console.WriteLine("  skillimg_tool wzset   <wzfile> <imgname> <nodepath> <value>");
        Console.WriteLine("  e.g. skillimg_tool setall 1511.img skill/15111007/level 3 attackCount");
        Console.WriteLine("  e.g. skillimg_tool setall 1511.img skill/15111007/level 500 damage");
        Console.WriteLine("  e.g. skillimg_tool wzset Skill.wz 1511.img skill/15111007/level/30/mobCount 15");
    }
}

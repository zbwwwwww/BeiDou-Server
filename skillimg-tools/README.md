# skillimg-tools — 技能 .img 数值修改工具

基于最小版 MapleLib（HaRepacker 的 wz 底层库裁剪编译），**替代手动开 HaRepacker 改技能数值**。
加密固定为 GMS 旧加密（HaRepacker 里「全球冒险岛(旧)」），与北斗客户端技能 img 一致。

## 文件说明

| 文件 | 用途 |
|---|---|
| `skillimg_tool.exe` | 命令行工具（运行主体） |
| `MapleLib_min.dll` | 运行依赖，最小 MapleLib 编译库 |
| `skillimg_tool.cs` | 工具源码（要改功能时用 build_maplib.rsp 重编译） |
| `build_maplib.rsp` | csc 编译响应文件（重编译命令见下） |
| `maplib_stubs.cs` | 第三方依赖 stub（编译必需） |

重编译（系统自带编译器，无需装 SDK）：
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe @build_maplib.rsp
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /out:skillimg_tool.exe /r:MapleLib_min.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll skillimg_tool.cs
```

## 命令格式

```
skillimg_tool read    <imgfile> <nodepath>                     查任意节点值
skillimg_tool set     <imgfile> <nodepath> <value>             改单点（节点需已存在）
skillimg_tool setall  <imgfile> <prefix> <value> [attrname]    批量改 level 组（缺节点自动新建，属性名默认 attackCount）
```

## 常用示例（奇袭者鲨鱼波 15111007，技能文件 ...\Data\Skill\1511.img）

```
# 只改 30 级段数为 3（满级技能只需改 30 级）
skillimg_tool.exe set "...\1511.img" skill/15111007/level/30/attackCount 3

# 全部 1~30 级段数改为 3
skillimg_tool.exe setall "...\1511.img" skill/15111007/level 3 attackCount

# 全部 1~30 级伤害改为 500
skillimg_tool.exe setall "...\1511.img" skill/15111007/level 500 damage

# 只改 30 级攻击距离为 1200
skillimg_tool.exe set "...\1511.img" skill/15111007/level/30/range 1200

# 只改 30 级打怪数量为 15
skillimg_tool.exe set "...\1511.img" skill/15111007/level/30/mobCount 15
```

## 注意事项

- 支持整数属性：`attackCount`、`damage`、`mobCount`、`range`、`mpCon`、`x/y`、`time` 等；字符串/向量/PNG 图像不做修改、原样保留。
- **改完必须完全退出客户端重进才生效**（技能数据启动时加载）。
- 服务端技能数据是解包 XML：`gms-server\wz\Skill.wz\1511.img.xml`；服务端 mobCount 丢包校验已在代码中注释（2026-10-18），客户端自定义 mobCount 不再被拦。
- 若改距离/范围超过服务端固定阈值（约 774px），服务端会打 DISTANCE_HACK 反作弊点（不拦伤害，但会累积），目前按需处理。

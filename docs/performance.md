# 我自己的 GTNH 性能优化

前提声明，每个人有不同的见解，我的优化方案不一定是最好的；  
如果你正在玩单人，推荐服务端客户端分离；

1. 进入 BIOS 关闭超线程（百度查询如何进入 BIOS）（为什么请看最下面）
2. 关闭 Hyper-V + VBS （右键左下角 Windows 图标，管理员 powershell，`bcdedit /set hypervisorlaunchtype off`）
3. 安装 neodymium (同时可以解决核显渲染问题) + \[optifine] (看个人) 或 angelica
4. 如果你正在用的是 Java17 的 GTNH，更换为 GraalVM EE 17 (<https://github.com/swseighman/Installing-GraalVM-Enterprise-Edition>)
5. 根据用的 Java 不同，修改启动器参数 <https://github.com/brucethemoose/Minecraft-Performance-Flags-Benchmarks>
6. 想尽一切办法提升单核性能，超频，换一台 PC（不要买洋垃圾，单核性能不好）

我自己用的参数：
```
-XX:+UnlockExperimentalVMOptions -XX:+UnlockDiagnosticVMOptions -XX:+AlwaysActAsServerClassMachine -XX:+AlwaysPreTouch -XX:+DisableExplicitGC -XX:+UseNUMA -XX:AllocatePrefetchStyle=3 -XX:NmethodSweepActivity=1 -XX:ReservedCodeCacheSize=400M -XX:NonNMethodCodeHeapSize=12M -XX:ProfiledCodeHeapSize=194M -XX:NonProfiledCodeHeapSize=194M -XX:-DontCompileHugeMethods -XX:+PerfDisableSharedMem -XX:+UseFastUnorderedTimeStamps -XX:+UseCriticalJavaThreadPriority -XX:+EagerJVMCI -Dgraal.TuneInlinerExploration=1 -Dgraal.CompilerConfiguration=enterprise -XX:+UseLargePages -XX:LargePageSizeInBytes=2m  -XX:+UseG1GC -XX:MaxGCPauseMillis=37 -XX:+PerfDisableSharedMem -XX:G1HeapRegionSize=16M -XX:G1NewSizePercent=23 -XX:G1ReservePercent=20 -XX:SurvivorRatio=32 -XX:G1MixedGCCountTarget=3 -XX:G1HeapWastePercent=20 -XX:InitiatingHeapOccupancyPercent=10 -XX:G1RSetUpdatingPauseTimePercent=0 -XX:MaxTenuringThreshold=1 -XX:G1SATBBufferEnqueueingThresholdPercent=30 -XX:G1ConcMarkStepDurationMillis=5.0 -XX:G1ConcRSHotCardLimit=16 -XX:G1ConcRefinementServiceIntervalMillis=150 -XX:GCTimeRatio=99  --illegal-access=warn -Djava.security.manager=allow -Dfile.encoding=UTF-8 --add-opens java.base/jdk.internal.loader=ALL-UNNAMED --add-opens java.base/java.net=ALL-UNNAMED --add-opens java.base/java.nio=ALL-UNNAMED --add-opens java.base/java.io=ALL-UNNAMED --add-opens java.base/java.lang=ALL-UNNAMED --add-opens java.base/java.lang.reflect=ALL-UNNAMED --add-opens java.base/java.text=ALL-UNNAMED --add-opens java.base/java.util=ALL-UNNAMED --add-opens java.base/jdk.internal.reflect=ALL-UNNAMED --add-opens java.base/sun.nio.ch=ALL-UNNAMED --add-opens jdk.naming.dns/com.sun.jndi.dns=ALL-UNNAMED,java.naming --add-opens java.desktop/sun.awt.image=ALL-UNNAMED --add-modules jdk.dynalink --add-opens jdk.dynalink/jdk.dynalink.beans=ALL-UNNAMED --add-modules java.sql.rowset --add-opens java.sql.rowset/javax.sql.rowset.serial=ALL-UNNAMED
```

## 为什么

- 众所周知，MC 吃单核，关闭超线程可以提升单核性能，但是降低多核性能（可以提升约 10% 单核性能）  
- 大部分 Windows 都会开 VBS，VBS 会让系统处于虚拟机环境下，降低约 30% 内存带宽，提高内存延迟，所以关闭，对大多数人不影响功能

经过这一套下来你大概能提升 50% - 100% fps（不做保证）
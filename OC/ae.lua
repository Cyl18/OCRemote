-- for k,v in pairs(c.me_controller.getCpus()[1]['cpu']) do print(k,v) end
-- for k,v in pairs() do print(k,v) end

function GetAllCpuInfo()
    local cpus = c.me_controller.getCpus()
    for _,v in pairs(cpus)do
        v["cpu"] = nil
    end
    return cpus
end

function Craft(name, damage, amount)
    return c.me_controller.getCraftables({name=name, damage=damage})[1].request(amount, true)

end

function CancelCrafting(cpuid)
    local cpu = c.me_controller.getCpus()[cpuid + 1]['cpu']
    cpu.cancel()
    return nil
end

function GetCpuInfo(cpuid)
    local cpu = c.me_controller.getCpus()[cpuid + 1]['cpu']
    local json = require('json')
    return json.encode(cpu.activeItems()) .. "^#^#"
        .. json.encode(cpu.storedItems()) .. "^#^#"
        .. json.encode(cpu.pendingItems()) .. "^#^#"
        .. json.encode(cpu.finalOutput())
end

function GetAllCpuFinalOutput()
    local cpus = c.me_controller.getCpus()
    local result = ""
    local n = 0
    for _, v in pairs(cpus) do
        result = result .. require("json").encode(v["cpu"].finalOutput()) .. "\n"
    end
    return result;
end

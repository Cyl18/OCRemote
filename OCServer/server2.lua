-- wget -f https://alist.cyan.cafe:2/d/intermediate/server.lua
-- wget -f https://alist.cyan.cafe:2/d/intermediate/client.lua
-- wget -f https://alist.cyan.cafe:2/d/intermediate/burner.lua
local internet = require('component').internet
local thread = require("thread")
local event = require("event")
local json = require('json')
local computer = require('computer')

while true do
    local v, m = xpcall(function()
        local req = internet.request("http://localhost:125/api/OCReport")
        local success = true
        local start = computer.uptime()
        while success do
            local status, err = req.finishConnect()
            if status then
                break
            end
            if status == nil then
                success = false
                break
            end

            if computer.uptime() >= start + 15 then
                req.close()
                success = false
                break
            end

            os.sleep(0)
        end

        if req.finishConnect() then
            local resp_code, _, _ = req.response()
            if resp_code == 200 then
                local result = ""
                while true do
                    local chunk, err = req.read()
                    if not chunk then break end
                    result = result .. chunk
                end
                local r = result
                local command_table = json.decode(r)
                local command_result_table = {}
                for cid, con in pairs(command_table) do
                    local failed = false
                    local command_result = ""
                    local command_id = cid
                    local command_content = con

                    local code, reason = load(command_content)
                    local cres = table.pack(xpcall(code, debug.traceback))
                    if not cres[1] then
                        failed = true
                        command_result = require("serialization").serialize("e$" .. cres[2])
                    else
                        command_result = require("serialization").serialize(cres[2])
                    end

                    local msg = require("serialization").unserialize(command_result)
                    if not failed then
                        msg = json.encode(msg)
                    end
                    command_result_table[cid] = msg
                end
                local req = internet.request("http://localhost:125/api/OCReport", json.encode(command_result_table), {
                    ["content-type"] = "text/plain"
                })
                local success = true
                local start = computer.uptime()
                while success do
                    local status, err = req.finishConnect()
                    if status then
                        break
                    end
                    if status == nil then
                        success = false
                    end

                    if computer.uptime() >= start + 1 then
                        req.close()
                        success = false
                    end

                    os.sleep(0)
                end
                if success then
                    req.close()
                end
            end
        end
        req.close()
    end, debug.traceback)
    if v == false then
        io.write("error: " .. m .. "\n")
    end
    os.sleep(0)
end

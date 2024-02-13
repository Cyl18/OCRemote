function getStorageEnergyStatus()
    
    return
    { c.proxy("a6802fe3-887d-41af-bbe8-12fe60073237").getSensorInformation() ,
        c.proxy("768fc26d-2bdc-432d-b34a-6704c7868570").getSensorInformation(),
        c.proxy("1316d9c7-6da3-478c-9c67-0d190a416c62").getSensorInformation() }
end

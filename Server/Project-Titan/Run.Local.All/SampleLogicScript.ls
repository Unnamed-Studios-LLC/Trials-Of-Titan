entity_state(name: "Test Enemy", actions: {
    wander(speed: 5)
    state(key: "state 1", actions: {
        set_state(key: "state 2", parent: 1)
    })
    state(key: "state 2", actions: {
        set_state(key: "state 1", parent: 1)
    })
})
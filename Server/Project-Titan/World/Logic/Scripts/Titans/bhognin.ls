entity_state(name: "Overworld Bhognin", actions: {
	state(name: "inactive", actions: {
		wander(speed: 3, period: 4)
		stay_near_spawn(speed: 6, distance: 20, enforce: 4, period: 8)
		if_health_below(percent: 0.95, true: {
			set_state(name: "activate", parent: 1)
		})
	})
	state(name: "activate", actions: {
		taunt(text: "rHahhble.. arrrhhhg..")
		add_effect(type: "Invulnerable", duration: 2, period: 1)
		wander(speed: 4, period: 0.4)
		if_elapsed(sec: 2, true: {
			set_state(name: "fight-start", parent: 1)
		})
	})
	state(name: "fight-start", actions: {
		taunt(text: "BhoooGniiinn!!")
		remove_effect(type: "Invulnerable")
		set_state(name: "spawn-chain", parent: 1)
	})
	state(name: "spawn-chain", actions: {
		spawn(name: "Bhognin's Chain", rate: 1, max: 1, radius: 0.5, arc: 182)
		if_elapsed(sec: 1, true: {
			set_state(name: "fight-swing", parent: 1)
		})
	})
	state(name: "fight-swing", actions: {
		set_texture(index: 2)
		wander(speed: 1, period: 0.25)
		if_elapsed(sec: 5, true: {
			set_state(name: "fight-throw", parent: 1)
		})
	})
	state(name: "fight-throw", actions: {
		set_texture(index: 1)
		if_elapsed(sec: 1.8, true: {
			set_state(name: "fight-swing", parent: 1)
		})
		shoot_player(index: 0, amount: 6)
	})
}, death: {
	create_gate(type: "bubra")
})
entity_state(name: "Bhognin's Chain", actions: {
	if_leader_count_divisible(value: 4, offset: -1, true: {
		set_texture(index: 1)
		set_size(value: 1.2)
		shoot_player(index: 0, period: 1.4, amount: 10)
	}, false: {
		set_texture(index: 0)
		set_size(value: 0.8)
	})
	if_topmost_dead(false: {
		if_minion(leaderCount: 13, false: {
			spawn(name: "Bhognin's Chain", rate: 1, max: 1, radius: 0.8, arc: 240, minionArcOffset: 24, minionArcExp: 0.1)
		})
	}, true: {
		if_elapsed(sec: 3, true: {
			despawn()
		})
	})
	if_minion(leaderCount: 2, false: {
		if_leader_state(name: "fight-swing", true: {
			orbit_leader(speed: 6, radius: 2)
		})
		if_leader_state(name: "fight-throw", true: {
			if_elapsed(sec: 0.7, false: {
				chase_angular(speed: 13, searchRadius: 16)
			})
		})
	}, true: {
		spring_chase_leader(distance: 0.5, acceleration: 3, drag: 1, velocityMax: 10)
	})
}, death: {
	give_minions_to_leader(ifTopLeader: "Bhognin", ifTopLeader: "Overworld Bhognin")
})
entity_state(name: "Bhognin", actions: {
	if_health_below(percent: 0.3, trigger: "true", true: {
		taunt(text: "BHOoo GNIIiinn aAARRrrhhhgggg!!")
		set_state(name: "spawn-chain-2")
	})
	state(name: "inactive", actions: {
		wander(speed: 3, period: 0.4)
		stay_near_spawn(speed: 6, distance: 3, enforce: 4, period: 8)
		if_health_below(percent: 0.95, true: {
			set_state(name: "activate", parent: 1)
		})
	})
	state(name: "activate", actions: {
		taunt(text: "rHahhble.. arrrhhhg..")
		add_effect(type: "Invulnerable", duration: 2, period: 1)
		wander(speed: 4, period: 0.4)
		if_elapsed(sec: 2, true: {
			set_state(name: "fight-start", parent: 1)
		})
	})
	state(name: "fight-start", actions: {
		taunt(text: "BhoooGniiinn!!")
		remove_effect(type: "Invulnerable")
		set_state(name: "spawn-chain", parent: 1)
	})
	state(name: "spawn-chain", actions: {
		spawn(name: "Bhognin's Chain", rate: 1, max: 1, radius: 0.5, arc: 182)
		if_elapsed(sec: 1, true: {
			set_state(name: "fight-swing", parent: 1)
		})
	})
	state(name: "spawn-chain-2", actions: {
		spawn(name: "Bhognin's Chain", rate: 2, max: 2, radius: 0.5, arc: 182)
		set_size(value: 1.7)
		if_elapsed(sec: 1, true: {
			set_state(name: "fight-swing", parent: 1)
		})
	})
	state(name: "fight-swing", actions: {
		set_texture(index: 2)
		wander(speed: 1, period: 0.25)
		if_elapsed(sec: 5, true: {
			set_state(name: "fight-throw", parent: 1)
		})
	})
	state(name: "fight-throw", actions: {
		set_texture(index: 1)
		if_elapsed(sec: 1.8, true: {
			set_state(name: "fight-swing", parent: 1)
		})
		shoot_player(index: 0, amount: 6, period: 1)
	})
}, death: {
	create_return_portal()
})
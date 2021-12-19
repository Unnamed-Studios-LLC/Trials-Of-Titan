entity_state(name: "Body of the Hydra", actions: {
	spawn(name: "Neck of the Hydra", rate: 3, max: 3, radius: 0)
	wander(speed: 4, period: 0.6)
	stay_on_tile(tile: "Deep Lake", tile: "Lake", speed: 6, distance: 5, period: 6, enforce: 3)
	shoot_player(amount: 14, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 2.8, periodMax: 3.6)
}, death: {
	chain_kill(name: "Neck of the Hydra")
	chain_kill(name: "Head of the Hydra")
})
entity_state(name: "Neck of the Hydra", actions: {
	state(name: "spawn", actions: {
		if_minion(leaderCount: 3, false: {
			spawn(name: "Neck of the Hydra", rate: 1, max: 1, radius: 0)
		}, true: {
			spawn(name: "Head of the Hydra", rate: 1, max: 1, radius: 3)
		})
		set_state(name: "link", parent: 1)
	})
	state(name: "link", actions: {
		linked_part()
	})
}, death: {
	chain_kill(name: "Neck of the Hydra")
	chain_kill(name: "Head of the Hydra")
})
entity_state(name: "Head of the Hydra", actions: {
	wander(speed: 4, period: 0.6)
	hover_leader_chase(speed: 5, minHover: 3, maxHover: 5, chaseDuration: 4, min: 1.5, topMost: "yes")
	shoot_player(index: 1, amount: 5, angleGap: 30, periodMin: 1.6, periodMax: 2.3)
	shoot_player(index: 2, amount: 2, angleGap: 10, periodMin: 1.2, periodMax: 1.6)
	shoot_player(index: 0, amount: 10, period: 3)
}, death: {
	chain_kill(name: "Neck of the Hydra")
})
entity_state(name: "Warrior Zebt", actions: {
	stay_on_tile(tile: "Deep Lake", tile: "Lake", tile: "Shallow Lake", speed: 6, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 16, enforce: 4, period: 8)
	spawn(name: "Zebtling", rate: 3, max: 3, radius: 2)
	spawn(name: "Zebtling", rate: 1, max: 3, radius: 2, period: 5)
	wander(speed: 2.5, period: 0.6)
	chase(speed: 2, min: 4)
	shoot_player(amount: 3, angleGap: 20, period: 2)
})
entity_state(name: "Zebtling", actions: {
	wander(speed: 5, period: 0.4)
	hover_leader_chase(speed: 5, minHover: 0, maxHover: 2.5, chaseDuration: 5, min: 0.8)
	shoot_player(index: 0, period: 1.3, angleOffset: 180, delay: 0)
	shoot_player(index: 1, period: 1.3, delay: 0)
})
entity_state(name: "Vicious Zebtling", actions: {
	wander(speed: 5.2, period: 0.4)
	hover_leader_chase(speed: 5.2, minHover: 0, maxHover: 2.5, chaseDuration: 5, min: 0.8)
	shoot_player(index: 1, period: 1.4, angleOffset: 180, delay: 0)
	shoot_player(index: 2, period: 1.4, delay: 0)
})
entity_state(name: "Zebt of Zornan", actions: {
	stay_on_tile(tile: "Deep Lake", tile: "Lake", tile: "Shallow Lake", speed: 6, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 16, enforce: 4, period: 8)
	spawn(name: "Zebtling", rate: 3, max: 3, radius: 2)
	spawn(name: "Zebtling", rate: 1, max: 3, radius: 2, period: 5)
	state(name: "wander", actions: {
		set_texture(index: 0)
		wander(speed: 2.5, period: 0.6)
		if_elapsed(sec: 10, true: {
			set_state(name: "spawn", parent: 1)
		})
	})
	state(name: "spawn", actions: {
		spawn(name: "Vicious Zebtling", rate: 2, max: 4, radius: 2, period: 0.4)
		if_elapsed(sec: 0.5, true: {
			if_elapsed(sec: 4, true: {
				if_elapsed(sec: 0.5, true: {
					set_state(name: "wander", parent: 1)
				}, false: {
					set_texture(index: 2)
				})
			}, false: {
				set_texture(index: 3)
			})
		}, false: {
			set_texture(index: 1)
		})
	})
})
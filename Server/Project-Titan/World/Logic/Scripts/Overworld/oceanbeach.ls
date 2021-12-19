entity_state(name: "Bandit Leader", actions: {
	spawn(name: "Bandit", rate: 2, max: 2)
	spawn(name: "Bald Bandit", rate: 1, max: 1)
	state(name: "walk", actions: {
		wander(speed: 3, period: 0.8)
		stay_near_spawn(speed: 12, distance: 12, enforce: 4, period: 10)
		set_texture(index: 0)
		if_elapsed(secMin: 3, secMax: 5, true: {
			set_state(name: "shoot", parent: 1)
		})
	})
	state(name: "shoot", actions: {
		set_texture(index: 1)
		if_elapsed(sec: 1, true: {
			shoot_player(index: 0, amount: 3, angleGap: 30)
			set_state(name: "walk", parent: 1)
		})
	})
})
entity_state(name: "Bald Bandit", actions: {
	wander(speed: 3, period: 0.8)
	shoot_player(index: 0, periodMin: 1.2, periodMax: 1.8)
	hover_leader(speed: 4, minHover: 1, maxHover: 6)
})
entity_state(name: "Bandit", actions: {
	wander(speed: 3, period: 0.8)
	shoot_player(index: 0, periodMin: 1.2, periodMax: 1.8)
	hover_leader_chase(speed: 5, minHover: 2, maxHover: 4, chaseDuration: 5)
})
entity_state(name: "Pirate Ship", actions: {
	spawn(name: "Pirate Dinghy", rate: 2, max: 2)
	wander(speed: 3, period: 0.8)
	stay_on_tile(tile: "Ocean Water", speed: 4, distance: 0, period: 0.01, enforce: 2)
	stay_near_spawn(speed: 12, distance: 12, enforce: 4, period: 10)
	shoot_player(index: 0, periodMin: 2, periodMax: 3, amount: 3, angleGap: 20)
})
entity_state(name: "Pirate Dinghy", actions: {
	wander(speed: 3, period: 0.8)
	hover_leader_chase(speed: 5, minHover: 1, maxHover: 5, chaseDuration: 5)
	shoot_player(index: 0, periodMin: 1.2, periodMax: 1.8)
})
entity_state(name: "Starfish", actions: {
	wander(speed: 3, period: 0.8)
	stay_near_spawn(speed: 8, distance: 8, enforce: 4, period: 10)
	chase(speed: 4, min: 1, period: 4)
	shoot_player(index: 0, period: 1.8, amount: 5)
})
entity_state(name: "Beach Crab", actions: {
	wander(speed: 4, period: 1.4)
	stay_near_spawn(speed: 8, distance: 8, enforce: 4, period: 10)
	chase(speed: 2, min: 1, period: 4)
	shoot_player(index: 0, period: 2.2, amount: 2, angleGap: 0)
	shoot_player(index: 1, period: 1.8, amount: 3, angleGap: 15)
})
entity_state(name: "Beach Snail", actions: {
	if_minion(false: {
		spawn(name: "Beach Snail", rate: 1, max: 1, radiusMin: 1, radiusMax: 2)
	})
	state(name: "inactive", actions: {
		if_player_within(distance: 7, true: {
			set_texture(index: 1)
			set_state(name: "grow", parent: 1)
		})
	})
	state(name: "grow", actions: {
		if_elapsed(sec: 0.62, true: {
			set_state(name: "fight", parent: 1)
		})
		wander(speed: 3, period: 0.8)
		stay_near_spawn(speed: 8, distance: 6, enforce: 4, period: 10)
	})
	state(name: "fight", actions: {
		set_texture(index: 0)
		wander(speed: 3, period: 0.8)
		stay_near_spawn(speed: 8, distance: 6, enforce: 4, period: 10)
		shoot_player(index: 0, period: 1.8, amount: 2, angleGap: 10)
	})
})
entity_state(name: "Beach Snake", actions: {
	spawn(name: "Beach Snake Egg", rate: 2, max: 2, radiusMin: 0.5, radiusMax: 2.5)
	spawn(name: "Beach Snake Baby", rate: 2, max: 2, radiusMin: 0.5, radiusMax: 2.5)
	wander(speed: 2, period: 0.8)
	stay_near_spawn(speed: 8, distance: 3, enforce: 4, period: 10)
	shoot_player(index: 0, periodMin: 1.0, periodMax: 1.4, amount: 2, angleGap: 0)
	chase(speed: 4, min: 1, searchRadius: 10)
})
entity_state(name: "Beach Snake Egg", actions: {
	do_nothing()
}, death: {
	death_spawn(name: "Beach Snake Baby", amount: 2)
})
entity_state(name: "Beach Snake Baby", actions: {
	wander(speed: 2, period: 0.3)
	stay_near_spawn(speed: 5, distance: 2, enforce: 2, period: 6)
	shoot_player(index: 0, periodMin: 0.8, periodMax: 1.2)
})
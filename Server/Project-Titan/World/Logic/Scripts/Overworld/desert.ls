entity_state(name: "Dune Snake", actions: {
	stay_on_tile(tile: "Desert Sand", speed: 4, distance: 5, period: 6, enforce: 3)
	spawn(name: "Dune Snake Body", rate: 1, max: 1, radius: 0.6, arc: 181)
	if_player_within(distance: 14, true: {
		chase_angular(speed: 5, angleRange: 45, period: 3, searchRadius: 14)
	}, false: {
		wander(speed: 5, period: 3)
	})
	state(name: "burrow", actions: {
		add_effect(type: "Invincible", duration: 2, period: 1)
		set_texture(index: 1)
		if_elapsed(sec: 4, true: {
			set_state(name: "unearth", parent: 1)
		})
	})
	state(name: "unearth", actions: {
		set_texture(index: 2)
		if_elapsed(sec: 0.75, true: {
			set_state(name: "fight", parent: 1)
		})
	})
	state(name: "fight", actions: {
		set_texture(index: 0)
		if_elapsed(sec: 6, true: {
			set_state(name: "burrow", parent: 1)
		})
		shoot_player(amount: 5, angleGap: 20, period: 2)
	})
})
entity_state(name: "Dune Snake Body", actions: {
	if_elapsed(sec: 1, false: {
		if_minion(leaderCount: 8, false: {
			spawn(name: "Dune Snake Body", rate: 1, max: 1, radius: 0.8, arc: 240, minionArcOffset: 24, minionArcExp: 0.1)
		})
	})
	if_leader_dead(false: {
		spring_chase_leader(distance: 0.5, acceleration: 3, drag: 1, velocityMax: 10)
	}, true: {
		wander(speed: 4, period: 0.6)
	})
	if_leader_state(name: "burrow", true: {
		add_effect(type: "Invincible", duration: 2, period: 1)
		set_texture(index: 1)
		if_elapsed(sec: 0.15, true: {
			set_state(name: "burrow", parent: 0)
		})
	})
	if_leader_state(name: "unearth", true: {
		set_texture(index: 2)
		if_elapsed(sec: 0.15, true: {
			set_state(name: "unearth", parent: 0)
		})
	})
	if_leader_state(name: "fight", true: {
		set_texture(index: 0)
		if_elapsed(sec: 0.15, true: {
			set_state(name: "fight", parent: 0)
		})
		shoot(amount: 5, period: 2, delay: -1, angleMin: 0, angleMax: 72)
	})
}, death: {
	give_minions_to_leader()
})
entity_state(name: "Dust Devil", actions: {
	stay_on_tile(tile: "Desert Sand", tile: "Desert Sand Light", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
	wander(speed: 5, period: 0.8)
	shoot_player(periodMin: 0.8, periodMax: 2)
})
entity_state(name: "Bubra Mule", actions: {
	spawn(name: "Bubra Mounted Warrior", rate: 1, max: 1, radius: 3)
	spawn(name: "Bubra Skilled Warrior", rate: 2, max: 2, radius: 3)
	spawn(name: "Bubra Warrior", rate: 3, max: 3, radius: 3)
	stay_on_tile(tile: "Desert Sand", tile: "Desert Sand Light", speed: 6, distance: 5, period: 6, enforce: 3)
	wander(speed: 1.25, period: 5)
	shoot_player(period: 4.5, amount: 16)
})
entity_state(name: "Bubra Mounted Warrior", actions: {
	hover_leader_chase(speed: 5, minHover: 1, maxHover: 2, chaseDuration: 4, min: 2)
	wander(speed: 4, period: 0.6)
	shoot_player(period: 4.5, amount: 14)
})
entity_state(name: "Bubra Skilled Warrior", actions: {
	hover_leader_chase(speed: 3.5, minHover: 2, maxHover: 3, chaseDuration: 5, min: 3)
	wander(speed: 2, period: 0.6)
	shoot_player(period: 1.2, amount: 1)
})
entity_state(name: "Bubra Warrior", actions: {
	hover_leader_chase(speed: 4.5, minHover: 2, maxHover: 4, chaseDuration: 5, min: 1)
	wander(speed: 3.2, period: 0.4)
	shoot_player(period: 1.1, amount: 2, angleGap: 20)
})
entity_state(name: "Scorpion", actions: {
	stay_on_tile(tile: "Desert Sand", tile: "Desert Sand Light", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
	wander(speed: 4, period: 0.8)
	chase(speed: 2.5, min: 1)
	shoot_player(period: 2)
	shoot_player(index: 1, amount: 7, angleGap: 15, period: 3.5)
})
entity_state(name: "Skeletal Warrior", actions: {
	spawn(name: "Skeletal Archer", rate: 2, max: 2, radius: 3)
	stay_on_tile(tile: "Desert Sand", tile: "Desert Sand Light", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
	wander(speed: 3, period: 0.8)
	chase(speed: 5, min: 1.5)
	shoot_player(periodMin: 1.4, periodMax: 2)
	shoot_player(index: 1, amount: 2, angleGap: 20, period: 2.5)
})
entity_state(name: "Skeletal Archer", actions: {
	hover_leader(speed: 4, minHover: 4, maxHover: 5)
	wander(speed: 4, period: 0.8)
	chase(speed: 3, min: 5)
	shoot_player(periodMin: 1.4, periodMax: 2, amount: 3, angleGap: 20)
})
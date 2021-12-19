entity_state(name: "Giant Spider", actions: {
	wander(speed: 2, period: 1)
	chase(speed: 1, min: 1)
	stay_on_tile(tile: "Mossy Grass", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)

	shoot_player(index: 0, period: 2, angleGap: 15, amount: 8)
	shoot_player(index: 1, period: 3, amount: 1, delay: 3)
})
entity_state(name: "Giant Spider Baby", actions: {
	wander(speed: 2, period: 0.3)
	chase(speed: 3, min: 1)
	hover_leader_chase(speed: 3, minHover: 0, maxHover: 4, chaseDuration: 6)

	shoot_player(index: 0, period: 1.4, amount: 1)
})
entity_state(name: "Guardian Ape", actions: {
	stay_on_tile(tile: "Mossy Grass", speed: 4, distance: 5, period: 6, enforce: 3)
	state(name: "chase", actions: {
		wander(speed: 1, period: 0.5)
		chase_angular(speed: 3.5, period: 1.5)
		shoot_player(index: 2, amount: 1, period: 1)
		if_elapsed(sec: 3, true: {
			random_state(name: "attack-1", name: "attack-2", parent: 1)
		})
	})
	state(name: "attack-1", actions: {
		shoot(index: 0, amount: 2, period: 0.6)
		shoot(index: 1, amount: 2, period: 0.6)
		shoot(index: 0, amount: 2, period: 0.6, delay: 0.3)
		shoot(index: 1, amount: 2, period: 0.6, delay: 0.3)
		shoot_player(index: 2, amount: 1, period: 1)
		if_elapsed(sec: 3, true: {
			set_state(name: "chase", parent: 1)
		})
	})
	state(name: "attack-2", actions: {
		shoot(index: 0, amount: 2, angle: 90, period: 0.6)
		shoot(index: 1, amount: 2, angle: 90, period: 0.6)
		shoot(index: 0, amount: 2, angle: 90, period: 0.6, delay: 0.3)
		shoot(index: 1, amount: 2, angle: 90, period: 0.6, delay: 0.3)
		shoot_player(index: 2, amount: 1, period: 1)
		if_elapsed(sec: 3, true: {
			set_state(name: "chase", parent: 1)
		})
	})
})
entity_state(name: "Floating Toxin", actions: {
	stay_on_tile(tile: "Mossy Grass", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
	if_minion(false: {
		spawn(name: "Floating Toxin", rate: 1, max: 1)
	}, true: {
		hover_leader(speed: 6, minHover: 4, maxHover: 6)
	})
	spawn(name: "Toxin Bomb", rate: 1, max: 2, period: 3)

	wander(speed: 3, period: 1)
	chase(speed: 1, min: 3)

	shoot_player(index: 0, period: 2, amount: 5, delay: 0)
	shoot_player(index: 1, period: 2, amount: 5, delay: 0)
})
entity_state(name: "Toxin Bomb", actions: {
	if_elapsed(sec: 5.5, false: {
		chase(speed: 5, min: 0.5)
	})
	if_elapsed(sec: 4, true: {
		set_flash(color: "red", duration: 10)
		if_elapsed(sec: 2, true: {
			shoot_player(index: 0, amount: 7)
			if_elapsed(sec: 0.15, true: {
				despawn()
			})
		})
	})
})
entity_state(name: "Pixie", actions: {
	stay_on_tile(tile: "Mossy Grass", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
	spawn(name: "Dark Pixie", rate: 1, max: 1)
	shoot_player(index: 0, period: 1.2)
	state(name: "zoom", actions: {
		wander(speed: 4, period: 2)
		if_elapsed(sec: 4, true: {
			set_state(name: "rest", parent: 1)
		})
	})
	state(name: "rest", actions: {
		wander(speed: 2, period: 0.4)
		chase(speed: 1, min: 3)
		if_elapsed(sec: 4, true: {
			set_state(name: "zoom", parent: 1)
		})
	})
})
entity_state(name: "Dark Pixie", actions: {
	shoot_player(index: 0, periodMin: 0.8, periodMax: 1.2)
	if_leader_state(name: "zoom", true: {
		orbit_leader(speed: 6, radius: 3)
	}, false: {
		orbit_player(speed: 6, radius: 3)
	})
})
entity_state(name: "Forest Droop", actions: {
	stay_on_tile(tile: "Mossy Grass", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
	state(name: "move", actions: {
		if_player_within(distance: 10, true: {
			chase(speed: 2, min: 1, searchRadius: 10)
			if_elapsed(sec: 3, true: {
				set_state(name: "shoot", parent: 1)
			})
		}, false: {
			wander(speed: 3, period: 1)
		})
	})
	state(name: "shoot", actions: {
		shoot_aoe_player(period: 2)
		if_elapsed(sec: 4, true: {
			set_state(name: "move", parent: 1)
		})
	})
})
entity_state(name: "Whispering Ent", actions: {
	stay_on_tile(tile: "Mossy Grass", speed: 4, distance: 5, period: 6, enforce: 3)
	state(name: "wander", actions: {
		set_texture(index: 0)
		wander(speed: 3, period: 3)
		if_player_within(distance: 9, true: {
			if_elapsed(sec: 3, true: {
				set_state(name: "shrink", parent: 1)
			})
		})
		shoot_player(amount: 10, period: 1.8)
	})
	state(name: "shrink", actions: {
		set_texture(index: 1)
		spawn(name: "Ent Sapling", rate: 12, max: 20, radius: 2)
		if_elapsed(sec: 7, true: {
			set_state(name: "grow", parent: 1)
		})
	})
	state(name: "grow", actions: {
		set_texture(index: 2)
		if_elapsed(sec: 0.6, true: {
			set_state(name: "wander", parent: 1)
		})
	})
})
entity_state(name: "Ent Sapling", actions: {
	shoot_player(angleOffsetMin: -20, angleOffsetMax: 20, period: 1.6)
	if_elapsed(sec: 7, true: {
		set_texture(index: 1)
		if_elapsed(sec: 0.4, true: {
			despawn()
		})
	})
})
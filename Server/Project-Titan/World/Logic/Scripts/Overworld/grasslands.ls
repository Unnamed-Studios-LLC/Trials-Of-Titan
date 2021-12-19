entity_state(name: "Goblin Brute", actions: {
	chase(speed: 3, min: 1)
	wander(speed: 0.5, period: 0.5)
	shoot_player(index: 0, angleGap: 15, amount: 7, period: 2.5)
	spawn(name: "Goblin Spearman", rate: 2, max: 2)
	spawn(name: "Goblin Archer", rate: 3, max: 3)
	stay_on_tile(tile: "Plain Grass", speed: 4, distance: 4, period: 4, enforce: 2)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
})
entity_state(name: "Goblin Spearman", actions: {
	wander(speed: 2, period: 0.5)
	hover_leader_chase(speed: 5, minHover: 0, maxHover: 3, chaseDuration: 5)
	shoot_player(index: 0, period: 1.5)
})
entity_state(name: "Goblin Archer", actions: {
	wander(speed: 2, period: 0.5)
	hover_leader(speed: 4, minHover: 4, maxHover: 6)
	shoot_player(index: 0, period: 2)
})
entity_state(name: "Flower Bug", actions: {
	if_minion(false: {
		spawn(name: "Flower Bug", rate: 2, max: 2, radiusMin: 1, radiusMax: 2)
	})
	state(name: "hiding", actions: {
		if_player_within(distance: 7, true: {
			set_texture(index: 1)
			set_state(name: "grow", parent: 1)
		})
	})
	state(name: "grow", actions: {
		if_elapsed(sec: 0.65, true: {
			set_texture(index: 0)
			set_state(name: "fight", parent: 1)
		})
	})
	state(name: "fight", actions: {
		chase(speed: 2, min: 5)
		wander(speed: 3, period: 1)
		stay_on_tile(tile: "Plain Grass", speed: 5, distance: 4, period: 4, enforce: 2)
		stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
		shoot_player(index: 0, period: 2)
	})
})
entity_state(name: "Rogue Summoner", actions: {
	spawn(name: "Wood Construct Inverse Orb", rate: 2, max: 2)
	spawn(name: "Wood Construct", rate: 1, max: 1)
	wander(speed: 2, period: 0.5)
	shoot_player(index: 0, angleGap: 36, amount: 10, period: 2)
	stay_on_tile(tile: "Plain Grass", speed: 4, distance: 4, period: 4, enforce: 2)
	stay_near_spawn(speed: 8, distance: 10, enforce: 4, period: 8)
})
entity_state(name: "Wood Construct", actions: {
	spawn(name: "Wood Construct Orb", rate: 1, max: 1)
	wander(speed: 2, period: 0.5)
	hover_leader_chase(speed: 3.5, minHover: 2, maxHover: 4, chaseDuration: 7)
	shoot_player(index: 0, angleGap: 36, amount: 10, period: 4)
})
entity_state(name: "Wood Construct Orb", actions: {
	spawn(name: "Wood Construct Inverse Orb", rate: 1, max: 1)
	wander(speed: 3, period: 0.5)
	hover_leader_chase(speed: 5, minHover: 1, maxHover: 3, chaseDuration: 4)
	shoot_player(index: 0, angleGap: 30, amount: 12, period: 2)
})
entity_state(name: "Wood Construct Inverse Orb", actions: {
	wander(speed: 2, period: 0.5)
	hover_leader(speed: 6, minHover: 0, maxHover: 1)
	shoot_player(index: 0, period: 2.5)
})
entity_state(name: "Demon Brute", actions: {
	stay_on_tile(tile: "Plain Grass", speed: 4, distance: 4, period: 4, enforce: 2)
	state(name: "chill", actions: {
		set_texture(index: 0)
		wander(speed: 2, period: 0.5)
		if_elapsed(sec: 1.5, true: {
			if_player_within(distance: 7, true: {
				set_state(name: "chase", parent: 1)
			})
		})
	})
	state(name: "chase", actions: {
		set_texture(index: 1)
		chase_angular(speed: 5, min: 0.5, period: 1)
		if_player_within(distance: 12, false: {
			set_state(name: "chill", parent: 1)
		})
		if_elapsed(sec: 6, true: {
			set_state(name: "chill", parent: 1)
		})
		shoot_player(index: 0, angle: 0, amount: 12, period: 2.2)
		shoot_player(index: 0, angle: 12, amount: 12, period: 2.2, delay: 0.3)
		shoot_player(index: 0, angle: 24, amount: 12, period: 2.2, delay: 0.6)

		shoot_player(index: 1, amount: 3, angleGap: 20, period: 2)
	})
})
entity_state(name: "Forest Wanderer", actions: {
	stay_on_tile(tile: "Plain Grass", speed: 4, distance: 4, period: 4, enforce: 2)
	state(name: "passive", actions: {
		wander(speed: 1, period: 2)
		if_player_within(distance: 10, true: {
			if_elapsed(sec: 2, true: {
				set_state(name: "agro", parent: 1)
			})
		})
	})
	state(name: "agro", actions: {
		state(name: "throw", actions: {
			set_state(name: "post-throw", parent: 1)
			if_minion_count_under(count: 4, true: {
				throw_minion_player(name: "Forest Protector", offset: 1.5, rand: 1.5, duration: 0.8)
				throw_minion_player(name: "Forest Protector", offset: 1.5, rand: 1.5, duration: 0.8)
				throw_minion_player(name: "Forest Protector", offset: 1.5, rand: 1.5, duration: 0.8)
				throw_minion_player(name: "Forest Protector", offset: 1.5, rand: 1.5, duration: 0.8)
			})
		})
		state(name: "post-throw", actions: {
			wander(speed: 2, period: 1)
			set_texture(index: 2)
			if_elapsed(sec: 2, true: {
				set_state(name: "charge", parent: 1)
			})
		})
		state(name: "charge", actions: {
			set_texture(index: 3)
			if_elapsed(sec: 1.5, true: {
				set_state(name: "grow-minions", parent: 1)
			})
			shoot(index: 1, angle: 0, amount: 12, delay: 0.2)
			shoot_player(index: 1, amount: 12, delay: 1.4)
		})
		state(name: "grow-minions", actions: {
			set_texture(index: 4)
			if_elapsed(sec: 0.8, true: {
				set_state(name: "pre-throw", parent: 1)
			})
		})
		state(name: "pre-throw", actions: {
			wander(speed: 2, period: 1)
			set_texture(index: 1)
			if_elapsed(sec: 2, true: {
				set_state(name: "throw", parent: 1)
			})
			shoot_player(index: 0, amount: 12, delay: 0.4)
			shoot_player(index: 1, amount: 3, angleGap: 40, delay: 1)
		})
	})
})
entity_state(name: "Forest Protector", actions: {
	if_player_within(distance: 8, true: {
		chase(speed: 4.5, min: 0.5, searchDistance: 8)
		wander(speed: 3, period: 0.5)
		shoot_player(index: 0, period: 1.2)
	}, false: {
		hover_leader(speed: 4, minHover: 1, maxHover: 3)
		wander(speed: 1.5, period: 0.5)
	})
})
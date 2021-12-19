entity_state(name: "Dumirian Chief", actions: {
	stay_on_tile(tile: "Full Snow", tile: "Partial Snow", tile: "Rock Snow", tile: "Frozen Lake", tile: "Cracked Lake", tile: "Cold Lake", speed: 6, distance: 5, period: 6, enforce: 3)
	state(name: "search", actions: {
		set_texture(index: 0)
		wander(speed: 3.5, period: 1)
		if_player_within(distance: 12, true: {
			set_state(name: "fight", parent: 1)
		})
	})
	state(name: "fight", actions: {
		set_texture(index: 0)
		wander(speed: 3.5, period: 1)
		shoot_player(index: 1, amount: 12, angleGap: 18, period: 1.3)
		if_elapsed(secMin: 3, secMax: 5, true: {
			set_state(name: "smash", parent: 1)
		})
	})
	state(name: "smash", actions: {
		set_flash(duration: 1.8)
		if_elapsed(sec: 1.8, true: {
			set_texture(index: 2)
			shoot_player(amount: 30, delay: 0)
			shoot_player(amount: 30, angleOffsetMin: 0, angleOffsetMax: 360, delay: 0.5)
			shoot_player(amount: 30, angleOffsetMin: 0, angleOffsetMax: 360, delay: 1)
			if_elapsed(sec: 1.5, true: {
				set_state(name: "search", parent: 1)
			})
		}, false: {
			set_texture(index: 1)
		})
	})
})
entity_state(name: "Dumirian Warrior", actions: {
	stay_on_tile(tile: "Full Snow", tile: "Partial Snow", tile: "Rock Snow", tile: "Frozen Lake", tile: "Cracked Lake", tile: "Cold Lake", speed: 6, distance: 5, period: 6, enforce: 3)
	state(name: "wander", actions: {
		stay_near_spawn(speed: 5, distance: 16, enforce: 4, period: 8)
		wander(speed: 3, period: 1)
		shoot_player(amount: 6, angleGap: 14, period: 1.2)
		shoot_player(amount: 2, angleGap: 180, period: 1.2)
		if_elapsed(secMin: 5, secMax: 7, true: {
			set_state(name: "chase", parent: 1)
		})
	})
	state(name: "chase", actions: {
		taunt(text: "For Balun!!")
		chase(speed: 5, min: 2, searchRadius: 11)
		wander(speed: 1, period: 0.4)
		shoot_player(amount: 6, angleGap: 25, period: 1.2, delay: 0.5)
		shoot_player(amount: 4, angleGap: 25, period: 1.2, delay: 0.9)
		shoot_player(amount: 8, angleGap: 25, period: 1.2, delay: 1.3)
		if_elapsed(sec: 3, true: {
			set_state(name: "wander", parent: 1)
		})
	})
})
entity_state(name: "Dumirian War Mammoth", actions: {
	stay_on_tile(tile: "Full Snow", tile: "Partial Snow", tile: "Rock Snow", tile: "Frozen Lake", tile: "Cracked Lake", tile: "Cold Lake", speed: 6, distance: 5, period: 6, enforce: 3)
	state(name: "wander", actions: {
		stay_near_spawn(speed: 8, distance: 16, enforce: 4, period: 8)
		wander(speed: 3, period: 0.8)
		set_texture(index: 0)
		if_elapsed(sec: 3.6, true: {
			if_player_within(distance: 10, true: {
				set_state(name: "prep", parent: 1)
			})
		})
	})
	state(name: "prep", actions: {
		set_texture(index: 1)
		charge(offset: 2.5, delay: 0.6, speed: 0.4, searchRadius: 10)
		set_flash(duration: 0.8)
		if_elapsed(sec: 1, true: {
			set_texture(index: 2)
			set_state(name: "charge", parent: 1)
		})
	})
	state(name: "charge", actions: {
		shoot_player(index: 0, amount: 6, angleGap: 15)
		if_elapsed(sec: 0.4, true: {
			set_state(name: "wander", parent: 1)
		})
	})
})
entity_state(name: "Brittlebone", actions: {
	stay_on_tile(tile: "Full Snow", tile: "Partial Snow", tile: "Rock Snow", tile: "Frozen Lake", tile: "Cracked Lake", tile: "Cold Lake", speed: 6, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 16, enforce: 4, period: 8)
	if_minion(false: {
		spawn(name: "Brittlebone", radius: 2, rate: 3, max: 3)
	})
	state(name: "hidden", actions: {
		if_player_within(distance: 8, true: {
			set_state(name: "rise", parent: 1)
		})
	})
	state(name: "rise", actions: {
		set_texture(index: 2)
		if_elapsed(sec: 0.86, true: {
			set_state(name: "chase", parent: 1)
		})
	})
	state(name: "chase", actions: {
		set_texture(index: 0)
		chase(speed: 4)
		wander(speed: 2, period: 0.5)
		shoot_player(angleOffsetMin: -15, angleOffsetMax: 15, periodMin: 0.8, periodMax: 1.4)
	})
})
entity_state(name: "Frost Leech", actions: {
	stay_on_tile(tile: "Full Snow", tile: "Partial Snow", tile: "Rock Snow", tile: "Frozen Lake", tile: "Cracked Lake", tile: "Cold Lake", speed: 6, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 16, enforce: 4, period: 8)
	wander(speed: 4, period: 0.25)
	spring_chase_player(distance: 0.5, acceleration: 3, drag: 1, velocityMax: 10, searchRadius: 9, period: 1)
	shoot_player(amount: 3, periodMin: 1.2, periodMax: 1.6)
})
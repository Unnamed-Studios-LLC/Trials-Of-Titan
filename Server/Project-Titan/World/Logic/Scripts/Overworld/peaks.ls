entity_state(name: "Grand Lich", actions: {
	spawn(name: "Greater Lich Skull", rate: 4, max: 4, period: 8)
	spawn(name: "Lich Skull", rate: 4, max: 9, period: 4)
	state(name: "normal", actions: {
		wander(speed: 3, period: 0.4)
		stay_near_spawn(speed: 8, distance: 3, enforce: 4, period: 8)
		set_texture(index: 0)
		if_elapsed(sec: 14, true: {
			set_state(name: "activated", parent: 1)
		})
		shoot_player(index: 5, period: 0.8, delay: 0.5)

		shoot(index: 0, amount: 4, angleGap: 90, angle: 0, period: 3, delay: 0.2)
		shoot(index: 1, amount: 4, angleGap: 90, angle: 0, period: 3, delay: 0.2)
		shoot(index: 2, amount: 4, angleGap: 90, angle: 0, period: 3, delay: 0.2)

		shoot(index: 0, amount: 4, angleGap: 90, angle: 45, period: 3, delay: 1.7)
		shoot(index: 1, amount: 4, angleGap: 90, angle: 45, period: 3, delay: 1.7)
		shoot(index: 2, amount: 4, angleGap: 90, angle: 45, period: 3, delay: 1.7)
	})
	state(name: "activated", actions: {
		set_texture(index: 1)
		if_elapsed(sec: 12, true: {
			set_state(name: "normal", parent: 1)
		})
		shoot(index: 4, amount: 4, angle: 45, angleGap: 90, period: 0.3)
		shoot(index: 3, amount: 7, angle: 45, angleGap: 12, period: 4, delay: 3)
		shoot(index: 3, amount: 7, angle: 135, angleGap: 12, period: 4, delay: 3.3)
		shoot(index: 3, amount: 7, angle: 225, angleGap: 12, period: 4, delay: 3.6)
		shoot(index: 3, amount: 7, angle: 315, angleGap: 12, period: 4, delay: 3.9)
	})
})
entity_state(name: "Greater Lich Skull", actions: {
	orbit_leader(speed: 4, radius: 6, periodMin: 5, periodMax: 8)
	shoot_player(amount: 2, angleGap: 0, period: 5)
})
entity_state(name: "Lich Skull", actions: {
	orbit_leader(speed: 9, radiusMin: 14, radiusMax: 18, periodMin: 5, periodMax: 8)
	shoot(amount: 2, angleGap: 180, angle: 0, period: 3)
	shoot(amount: 2, angleGap: 180, angle: 180, period: 3, delay: 1.5)
})
entity_state(name: "Stranded Knight", actions: {
	stay_on_tile(tile: "Mountain Rock", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 16, enforce: 4, period: 8)
	state(name: "chase", actions: {
		wander(speed: 3, period: 2)
		set_texture(index: 0)
		if_player_within(distance: 12, true: {
			chase(speed: 5.5, min: 1)
			if_elapsed(sec: 3, true: {
				set_texture(index: 1)
				set_state(name: "attack", parent: 1)
			})
		})
	})
	state(name: "attack", actions: {
		shoot_player(amount: 12)
		if_elapsed(sec: 0.5, true: {
			set_state(name: "chase", parent: 1)
		})
	})
})
entity_state(name: "Conjurer of Doom", actions: {
	stay_on_tile(tile: "Mountain Rock", speed: 4, distance: 5, period: 6, enforce: 3)
	stay_near_spawn(speed: 8, distance: 16, enforce: 4, period: 8)
	shoot_player(amount: 10, periodMin: 1.8, periodMax: 2.4)
	state(name: "wander", actions: {
		wander(speed: 3, period: 0.8)
		if_elapsed(sec: 8, true: {
			if_player_within(distance: 9, true: {
				set_state(name: "spawn", parent: 1)
			})
		})
	})
	state(name: "spawn", actions: {
		spawn(name: "Conjured Bone Reaper", rate: 1, max: 1, radius: 2)
		if_elapsed(sec: 0.5, true: {
			set_state(name: "wander", parent: 1)
		})
	})
})
entity_state(name: "Conjured Bone Reaper", actions: {
	hover_leader_chase(speed: 4, minHover: 2, maxHover: 4, chaseDuration: 7)
	wander(speed: 4, period: 0.4)
	if_elapsed(sec: 1.5, true: {
		shoot_player(index: 0, amount: 2, angleGap: 15, period: 2.8)
		shoot_player(index: 0, amount: 2, angleGap: 15, period: 2.8, delay: 0.5)
		shoot_player(index: 1, amount: 3, angleGap: 25, periodMin: 2, periodMax: 3)
	})
})
entity_state(name: "Black Knight", actions: {
	state(name: "wander", actions: {
		stay_on_tile(tile: "Mountain Rock", speed: 4, distance: 5, period: 6, enforce: 3)
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
		charge(offset: 2, delay: 0.7, speed: 0.3, searchRadius: 10)
		if_elapsed(sec: 1.05, true: {
			set_texture(index: 2)
			random_state(name: "lunge-single", name: "lunge-spread", parent: 1)
		})
	})
	state(name: "lunge-single", actions: {
		shoot_player(index: 0, amount: 2, angleGap: 15, delay: 0.3)
		shoot_player(index: 1, amount: 2, angleGap: 15, delay: 0.3)
		shoot_player(index: 2, amount: 2, angleGap: 15, delay: 0.3)
		shoot_player(index: 3, amount: 2, angleGap: 15, delay: 0.3)
		if_elapsed(sec: 0.7, true: {
			set_state(name: "wander", parent: 1)
		})
	})
	state(name: "lunge-spread", actions: {
		shoot_player(index: 1, amount: 5, angleGap: 20, delay: 0.3)
		shoot_player(index: 3, amount: 5, angleGap: 20, delay: 0.3)
		if_elapsed(sec: 0.7, true: {
			set_state(name: "wander", parent: 1)
		})
	})
})
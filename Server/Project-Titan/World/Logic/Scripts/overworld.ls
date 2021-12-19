entity_state(name: "Evil Bat", actions: {
	timed_switch(period: 4, delay: 4, actions: {
		shoot(index: 0, angle: 270, angleGap: 60, amount: 3, period: 2, delay: 0)
	})
})

entity_state(name: "Giant Frog", actions: {
	if_player_within(distance: 8, true: {
		chase_angular(speed: 4, angleRange: 60, period: 1.5)
	}, false: {
		wander(speed: 4, period: 1.5)
	})
})
entity_state(name: "Balun", actions: {
	stay_near_spawn(speed: 12, distance: 12, enforce: 4, period: 10)
	state(name: "wander", actions: {
		add_effect(type: "Invulnerable", duration: 10, period: 9)
		wander(speed: 3, period: 1)
		if_player_within(distance: 10, true: {
			if_elapsed(sec: 5, true: {
				set_state(name: "init-fight", parent: 1)
			})
		})
	})
	state(name: "init-fight", actions: {
		remove_effect(type: "Invulnerable")
		set_state(name: "random-fight", parent: 1)
	})
	state(name: "random-fight", actions: {
		wander(speed: 2, period: 0.5)
		set_texture(index: 0)

		shoot(index: 1, angle: 0, angleGap: 60, amount: 6, period: 1.2)
		shoot(index: 1, angle: 40, angleGap: 60, amount: 6, period: 1.2, delay: 0.4)
		shoot(index: 1, angle: 20, angleGap: 60, amount: 6, period: 1.2, delay: 0.8)

		if_elapsed(sec: 5, true: {
			random_state(parent: 1, name: "fight-2", name: "fight-2")
		})
	})
	state(name: "fight-1", actions: {
		wander(speed: 2, period: 0.5)
		set_texture(index: 1)
		
		throw_minion_player(name: "Lightning", offset: 0.2, rand: 1.5, duration: 0.1, period: 0.5, target: "Random")

		if_elapsed(sec: 8, true: {
			set_state(name: "random-fight", parent: 1)
		})
	})
	state(name: "fight-2", actions: {
		set_state(name: "fight-2-charge", parent: 1)
	})
	state(name: "fight-2-charge", actions: {
		set_texture(index: 2)
		set_flash(duration: 1.5)
		charge(offset: 0, delay: 1.4, speed: 0.2)
		set_size(value: 1.6)
		if_elapsed(sec: 1.6, true: {
			set_texture(index: 3)
			set_state(name: "fight-2-slam", parent: 1)
		})
	})
	state(name: "fight-2-slam", actions: {
		set_size(value: 1.3)
		shoot(index: 3, angle: 0, angleGap: 30, amount: 12)
		shoot(index: 3, angle: 20, angleGap: 30, amount: 12, delay: 0.4)
		shoot(index: 3, angle: 10, angleGap: 30, amount: 12, delay: 0.8)

		if_elapsed(sec: 1.2, true: {
			set_state(name: "random-fight", parent: 1)
		})
	})
})
entity_state(name: "Lightning", actions: {
	if_elapsed(sec: 0.38, true: {
		shoot(angleGap: 72, amount: 5)
		shoot(angle: 36, angleGap: 72, amount: 5, delay: 0.2)
	})
	if_elapsed(sec: 0.54, true: {
		set_texture(index: 1)
		if_elapsed(sec: 0.24, true: {
			despawn()
		})
	})
})
entity_state(name: "Dragon", actions: {
	state(name: "wander", actions: {
		set_hover(value: 0)
		set_texture(index: 0)
		set_size(value: 1, rate: 1)
		wander(speed: 4, period: 0.65)
		shoot_player(index: 0, angleGap: 30, amount: 12, period: 2)
		shoot_player(index: 1, angleGap: 45, amount: 8, period: 2, delay: 1)
		if_elapsed(sec: 6, true: {
			set_state(name: "chase", parent: 1)
		})
	})
	state(name: "chase", actions: {
		set_hover(value: 0.5)
		set_texture(index: 1)
		set_size(value: 1.2, rate: 1)
		wander(speed: 3, period: 0.5)
		chase(speed: 5, min: 2, searchRadius: 12)
		shoot_player(index: 0, angleGap: 30, amount: 12, period: 2)
		shoot_player(index: 1, angleGap: 45, amount: 8, period: 2, delay: 1)
		shoot_player(index: 2, angleGap: 20, amount: 2, period: 1.5, delay: 0.5)
		if_elapsed(sec: 6, true: {
			set_state(name: "wander", parent: 1)
		})
	})
})
entity_state(name: "Orange Cat", actions: {
	chase(speed: 3, min: 1)
	wander(speed: 0.5, period: 0.5)
})
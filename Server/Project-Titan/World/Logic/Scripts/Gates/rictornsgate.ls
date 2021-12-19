entity_state(name: "Mysterious Butterfly", actions: {
	orbit_player(speed: 6, radius: 4)
	wander(speed: 2, period: 0.5)
	shoot_player(amount: 6, periodMin: 1.8, periodMax: 2.6)
})
entity_state(name: "Scout Tower", actions: {
	state(name: "inactive", actions: {
		if_player_within(distance: 8, true: {
			set_state(name: "activated", parent: 1)
		})
	})
	state(name: "activated", actions: {
		emote(type: "Danger")
		shoot_player(amount: 5, angleGap: 20, periodMin: 1.2, periodMax: 1.8)
	})
})
entity_state(name: "Mage Tower", actions: {
	state(name: "inactive", actions: {
		if_player_within(distance: 9, true: {
			set_state(name: "activated", parent: 1)
		})
	})
	state(name: "activated", actions: {
		emote(type: "Danger")
		shoot_player(index: 0, amount: 1, angleGap: 0, period: 0.3)
		shoot_player(index: 1, amount: 3, angleGap: 30, periodMin: 1.2, periodMax: 1.8)
	})
})
entity_state(name: "Scout of Rictorn", actions: {
	state(name: "inactive", actions: {
		if_player_within(distance: 8, true: {
			set_state(name: "activated", parent: 1)
		})
	})
	state(name: "activated", actions: {
		emote(type: "Danger")
		wander(speed: 3, period: 0.6)
		shoot_player(amount: 2, angleGap: 20, periodMin: 1.4, periodMax: 2)
	})
})
entity_state(name: "Mage of Rictorn", actions: {
	state(name: "inactive", actions: {
		if_player_within(distance: 9, true: {
			set_state(name: "activated", parent: 1)
		})
	})
	state(name: "activated", actions: {
		emote(type: "Danger")
		wander(speed: 2, period: 0.6)
		shoot_player(index: 0, amount: 1, angleGap: 0, period: 0.4)
		shoot_player(index: 1, amount: 2, angleGap: 30, periodMin: 1.3, periodMax: 1.9)
	})
})
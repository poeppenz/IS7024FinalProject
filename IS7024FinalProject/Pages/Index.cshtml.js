        document.addEventListener('DOMContentLoaded', () => {
            const searchInput = document.getElementById('autocomplete-input');
            const resultsContainer = document.getElementById('autocomplete-results');
            let debounceTimer;

            // Function to fetch and display results
            const fetchResults = async (query) => {
                resultsContainer.innerHTML = '<div class="list-group-item text-center text-muted p-2 rounded-0">Searching...</div>';
                
                const url = '/?handler=Autocomplete&term=' + encodeURIComponent(query);

                try {
                    const response = await fetch(url);
                    const data = await response.json();

                    resultsContainer.innerHTML = ''; // Clear loading indicator

                    if (data && data.length > 0) {
                        data.forEach((event, index) => {
                            const resultItem = document.createElement('a');
                            resultItem.href = '/ParkingSearch?eventId=' + event.id;
                            resultItem.classList.add(
                                'list-group-item', 
                                'list-group-item-action', 
                                'p-3', 
                                'border-top-0',
                                'text-left'
                            );
                            
                            if (index === 0) resultItem.classList.add('rounded-top-3');
                            if (index === data.length - 1) resultItem.classList.add('rounded-bottom-3');

                            resultItem.innerHTML = 
                                '<div>' +
                                    '<strong class="text-primary d-block">' + event.title + '</strong>' +
                                    '<small class="text-muted">' +
                                        '@ ' + event.venue + ' on ' + event.date +
                                    '</small>' +
                                '</div>';
                            
                            resultsContainer.appendChild(resultItem);
                        });
                    } else {
                        resultsContainer.innerHTML = '<div class="list-group-item text-center text-muted rounded-3">No matching events found.</div>';
                    }
                } catch (error) {
                    console.error('Autocomplete fetch error:', error);
                    // Update the error message to be more generic and user-friendly
                    resultsContainer.innerHTML = '<div class="list-group-item text-danger rounded-3">Error fetching results. Please try again.</div>';
                }
            };

            // Debounce the input event to limit API calls
            searchInput.addEventListener('keyup', (e) => {
                const query = e.target.value.trim();
                
                clearTimeout(debounceTimer);

                // Start searching only after 3 characters are typed
                if (query.length >= 3) {
                    debounceTimer = setTimeout(() => {
                        fetchResults(query);
                    }, 300); // 300ms debounce
                } else {
                    // Hide results if the query is too short or cleared
                    resultsContainer.innerHTML = '';
                }
            });

            // Hide results when clicking outside
            document.addEventListener('click', (e) => {
                if (!resultsContainer.contains(e.target) && e.target !== searchInput) {
                    resultsContainer.innerHTML = '';
                }
            });
        });